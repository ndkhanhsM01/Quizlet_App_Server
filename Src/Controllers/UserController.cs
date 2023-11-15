using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Models.Helper;
using Quizlet_App_Server.Services;
using Quizlet_App_Server.Utility;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Quizlet_App_Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerExtend<User>
    {
        private readonly UserService service;
        public UserController(UserStoreDatabaseSetting setting, IMongoClient mongoClient, IConfiguration config) 
            : base(setting, mongoClient)
        {
            service = new(mongoClient, config);
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        // GET: api/<UserController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
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

            //string token = service.GenerateToken(existingUser);
            return Ok(existingUser);
        }

        // POST api/<UserController>
        [HttpPost]
        public ActionResult<User> SignUp([FromBody] UserSignUp request)
        {
            User newUser = new User() { LoginName = request.LoginName, LoginPassword = request.LoginPassword };
            
            // validate user
            var existingDocument = service.FindByLoginName(request.LoginName); 

            if (existingDocument != null)
            {
                return BadRequest("Username already exists");
            }

            // insert new user's information
            newUser.SeqId = service.GetNextID();
            collection.InsertOne(newUser);

            return Ok(newUser);
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
    }
}
