using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Services;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StudySetPublicController: ControllerExtend<User>
    {
        protected readonly UserService userService;
        public StudySetPublicController(UserStoreDatabaseSetting setting, IMongoClient mongoClient, IConfiguration config)
            : base(setting, mongoClient)
        {
            userService = new(mongoClient, config);
        }

        [HttpGet]
        public ActionResult<StudySetPublic> GetOne(string userId, string setId)
        {
            var result = userService.GetOneStudySetPublic(userId, setId);

            if(result == null)
            {
                return NotFound("userId or setId incorrect");
            }

            return new ActionResult<StudySetPublic>(result);
        }

        [HttpGet]
        public ActionResult<List<StudySetPublic>> GetAll(string userId)
        {
            var result = userService.GetAllStudySetPublic(userId);

            if (result == null)
            {
                return NotFound("userId incorrect");
            }

            return new ActionResult<List<StudySetPublic>>(result);
        }
    }
}
