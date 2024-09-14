using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Models.Helper;
using Quizlet_App_Server.Services;
using Quizlet_App_Server.Src.Models.OtherFeature.Notification;
using Quizlet_App_Server.Src.Models.OtherFeature.RankSystem;
using Quizlet_App_Server.Src.Services;
using Quizlet_App_Server.Utility;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Quizlet_App_Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerExtend<User>
    {
        private readonly UserService service;
        private readonly RankSystemService rankSystemService;
        public UserController(UserStoreDatabaseSetting setting, IMongoClient mongoClient, IConfiguration config) 
            : base(setting, mongoClient)
        {
            service = new(mongoClient, config);
            rankSystemService = new(mongoClient, config);
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        // GET: api/<UserController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        [HttpGet]
        public ActionResult<RankResult> GetRankResult(string userId)
        {
            var result = rankSystemService.GetRankResult(userId);

            if (result == null) return BadRequest("Request faild");

            return new ActionResult<RankResult>(result);
        }
        [HttpGet]
        public ActionResult<List<Notification>> GetAllCurrentNotices(string userId)
        {
            var existingUser = service.FindById(userId);
            if (existingUser == null)
            {
                return NotFound("User ID not found");
            }

            var result = existingUser.AllNotices;

            return new ActionResult<List<Notification>>(result);
        }
        // GET api/<UserController>/5
        [HttpPost]
        public ActionResult<User> Login([FromBody] UserLogin loginRequest)
        {
            // find user
            var existingUser = service.FindByLoginName(loginRequest.LoginName);

            // login name incorrect
            if(existingUser == null)
            {
                return NotFound("Login name not found");
            }

            // password incorrect
            if(!existingUser.LoginPassword.Equals(loginRequest.LoginPassword))
            {
                return BadRequest("Password incorrect");
            }

            #region detect new version of achievement
            Achievement currentAchievement = existingUser.Achievement != null 
                                            ? existingUser.Achievement
                                            : new Achievement();
            Achievement configAchievement = service.GetConfigData<Achievement>("Achievement");

            if(configAchievement.Version > currentAchievement.Version)
            {
                List<Models.Task> newTasks = new List<Models.Task>();

                foreach (var configTask in configAchievement.TaskList)
                {
                    var taskOfUser = currentAchievement.TaskList.Find(t => t.Id == configTask.Id);

                    if (taskOfUser == null)
                    {
                        newTasks.Add(configTask);
                    }
                    else if (taskOfUser.Condition != configTask.Condition)
                    {
                        taskOfUser.Condition = configTask.Condition;
                    }
                }

                currentAchievement.Version = configAchievement.Version;
                currentAchievement.TaskList.AddRange(newTasks);
                existingUser.Achievement = service.UpdateAchievement(existingUser.Id, currentAchievement).Achievement;

            }
            #endregion
            //string token = service.GenerateToken(existingUser);
            return Ok(existingUser);
        }
        // GET api/<UserController>/5
        [HttpPost]
        public ActionResult<User> GetInfoByID(string ID)
        {
            // find user
            var existingUser = service.FindById(ID);

            // login name incorrect
            if (existingUser == null)
            {
                return NotFound("User's ID not found");
            }

            return Ok(existingUser);
        }
        // POST api/<UserController>
        [HttpPost]
        public ActionResult<User> SignUp([FromBody] UserSignUp request)
        {
            User newUser = new User()
            { 
                LoginName = request.LoginName, 
                LoginPassword = request.LoginPassword ,
                UserName = request.LoginName,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth
            };

            // validate user
            var existingDocument = service.FindByLoginName(request.LoginName); 

            if (existingDocument != null)
            {
                return BadRequest("Username already exists");
            }

            // insert new user's information
            newUser.SeqId = service.GetNextID();
            newUser.Achievement = service.GetConfigData<Achievement>("Achievement");
            collection.InsertOne(newUser);

            return Ok(newUser);
        }
        [HttpPost]
        public ActionResult<StreakRespone> DetectContinueStudy(string userId, long timeDetect)
        {
            var existingUser = service.FindById(userId);
            if (existingUser == null)
            {
                return NotFound("User ID not found");
            }

            timeDetect = TimeHelper.UnixTimeNow;
            // caculate streak
            if(existingUser.Streak == null || existingUser.Streak.LastTime <= 0)
            {
                existingUser.Streak = new Streak();
            }
            else
            {
                var lastTimeStudy = TimeHelper.ToDateTime(existingUser.Streak.LastTime);
                var newTimeDetect = TimeHelper.ToDateTime((int)timeDetect);

                lastTimeStudy = new DateTime(lastTimeStudy.Year, lastTimeStudy.Month, lastTimeStudy.Day);
                newTimeDetect = new DateTime(newTimeDetect.Year, newTimeDetect.Month, newTimeDetect.Day);

                TimeSpan timeOffset = newTimeDetect - lastTimeStudy;
                if(timeOffset.TotalDays == 1)
                {
                    existingUser.UpdateStreak();
                }
                else if(timeOffset.TotalDays > 1)
                {
                    existingUser.Streak.CurrentStreak = 1;
                }

            }

            // update last time caculate
            existingUser.Streak.LastTime = timeDetect;

            #region Update achivement
            int hour = TimeHelper.ToDateTime(timeDetect).Hour;
            if (hour >= 22 && hour < 24)
            {
                var taskLateNight = existingUser.Achievement.TaskList.Find(t => t.Id == 201);
                if(taskLateNight != null)
                {
                    bool wasCompleted = taskLateNight.Status >= Models.TaskStatus.Completed;
                    taskLateNight.Progress++;

                    if (!wasCompleted && taskLateNight.Status >= Models.TaskStatus.Completed)
                        //existingUser.UpdateScore(taskLateNight.Score ?? 0);
                        existingUser.CompleteNewTask(taskLateNight);
                }
            }
            else if (hour >= 4 && hour < 7)
            {
                var taskEarly = existingUser.Achievement.TaskList.Find(t => t.Id == 202);
                if (taskEarly != null)
                {
                    bool wasCompleted = taskEarly.Status >= Models.TaskStatus.Completed;
                    taskEarly.Progress++;

                    if (!wasCompleted && taskEarly.Status >= Models.TaskStatus.Completed)
                        //existingUser.UpdateScore(taskEarly.Score ?? 0);
                        existingUser.CompleteNewTask(taskEarly);
                }
            }
            
            var taskStudyCard = existingUser.Achievement.TaskList.Find(t => t.Id == 200);
            if (taskStudyCard != null)
            {
                bool wasCompleted = taskStudyCard.Status >= Models.TaskStatus.Completed;
                taskStudyCard.Progress++;

                if (!wasCompleted && taskStudyCard.Status >= Models.TaskStatus.Completed)
                    //existingUser.UpdateScore(taskStudyCard.Score ?? 0);
                    existingUser.CompleteNewTask(taskStudyCard);
            }
            #endregion
            // update in database
            var update = Builders<User>.Update
                .Set("streak", existingUser.Streak)
                .Set("achievement", existingUser.Achievement)
                .Set("collection_storage", existingUser.CollectionStorage)
                .Set("all_notices", existingUser.AllNotices);
            var filter = Builders<User>.Filter.Eq(x => x.Id, userId);

            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };
            var updatedUser = collection.FindOneAndUpdate(filter, update, options);
            StreakRespone result = new StreakRespone()
            {
                Streak = updatedUser.Streak,
                Achievement= updatedUser.Achievement
            };

            return new ActionResult<StreakRespone>(result);
        }
        // PUT api/<UserController>/5
        [HttpPut]
        public ActionResult<User> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
        {
            // validate user
            var existingUser = service.FindById(id);

            if (existingUser == null)
            {
                return NotFound("User ID not found");
            }
            else if (!existingUser.LoginPassword.Equals(request.OldPassword))
            {
                return BadRequest("Old password incorrect");
            }
            else if (existingUser.LoginPassword.Equals(request.NewPassword))
            {
                return BadRequest("New password is same the current password");
            }

            var update = Builders<User>.Update.Set("login_password", request.NewPassword);
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var result = collection.UpdateOne(filter, update);

            return Ok("Change password successful");
        }
        [HttpPut]
        public ActionResult<InfoPersonal> UpdateInfo(string userId, [FromBody] InfoPersonal req)
        {
            User userExisting = service.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            var result = service.UpdateInfoUser(userId, req);
            return new ActionResult<InfoPersonal>(result);
        }
        [HttpDelete] 
        public ActionResult DeleteAllUser(string password)
        {
            if (!password.Equals("qzl_nice_app")) return BadRequest("Wrong password");

            long deleteCount = service.DeleteAllUser();
            return Ok($"Deleted: {deleteCount}");
        }
    }
}
