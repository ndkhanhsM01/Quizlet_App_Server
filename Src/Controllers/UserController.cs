using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Models.Helper;
using Quizlet_App_Server.Utility;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Quizlet_App_Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerExtend<User>
    {
        public UserController(UserStoreDatabaseSetting setting, IMongoClient mongoClient) 
            : base(setting, mongoClient)
        {
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public int GetNextID()
        {
            string id = "user_sequence";
            var database = client.GetDatabase(VariableConfig.DatabaseName);
            var sequenceCollection = database.GetCollection<UserSequence>(VariableConfig.Collection_UserSequence);

            var filter = Builders<UserSequence>.Filter.Eq(x => x.Id, id);
            var existingDocument = sequenceCollection.Find(filter).FirstOrDefault();

            if (existingDocument == null)
            {
                var defaultSequence = new UserSequence
                {
                    Id = id,
                    Value = 10000
                };

                sequenceCollection.InsertOne(defaultSequence);
                return defaultSequence.Value;
            }

            var update = Builders<UserSequence>.Update.Inc(x => x.Value, 1);
            var options = new FindOneAndUpdateOptions<UserSequence>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            var result = sequenceCollection.FindOneAndUpdate<UserSequence>(filter, update, options);
            return result.Value;
        }
        // GET: api/<UserController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<UserController>/5
        [HttpGet]
        public ActionResult<User> Login([FromQuery] UserLogin loginRequest)
        {
            // find user
            var filter = Builders<User>.Filter.Eq(x => x.LoginName, loginRequest.LoginName);
            var existingUser = collection.Find(filter).FirstOrDefault();

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

            return Ok(existingUser);
        }

        // POST api/<UserController>
        [HttpPost]
        public ActionResult<User> SignUp([FromBody] UserSignUp request)
        {
            User newUser = new User() { LoginName = request.LoginName, LoginPassword = request.LoginPassword };
            
            // validate user

            var filter = Builders<User>.Filter.Eq(x => x.LoginName, request.LoginName);
            var existingDocument = collection.Find(filter).FirstOrDefault();

            if (existingDocument != null)
            {
                return BadRequest("Username already exists");
            }

            // insert new user's information
            newUser.UserId = this.GetNextID();
            collection.InsertOne(newUser);

            return Ok(newUser);
        }

        // PUT api/<UserController>/5
        [HttpPut("{userId}")]
        public ActionResult<User> ChangePassword(int userId, [FromBody] ChangePasswordRequest request)
        {
            // validate user
            var filter = Builders<User>.Filter.Eq(x => x.UserId, userId);
            var existingUser = collection.Find(filter).FirstOrDefault();

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
            var result = collection.UpdateOne(filter, update);

            return Ok("Change password successful");
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
