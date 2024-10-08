using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Src.Services;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        protected readonly AdminService service;
        protected readonly IMongoCollection<Admin> collection;
        protected readonly IMongoClient client;
        public AdminController(IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(VariableConfig.DatabaseName);
            collection = database.GetCollection<Admin>(VariableConfig.Collection_Admin);

            this.client = mongoClient;
            this.service = new(mongoClient, config);
        }
        [HttpPost]
        public ActionResult<Admin> SignUp([FromBody] AdminSignUp request)
        {
            Admin newAccount = new Admin()
            {
                LoginName = request.LoginName,
                LoginPassword = request.LoginPassword,
                UserName = request.LoginName,
                Email = request.Email
            };

            // validate account
            var existingDocument = service.FindByLoginName(newAccount.LoginName);

            if (existingDocument != null)
            {
                return BadRequest("Username already exists");
            }

            collection.InsertOne(newAccount);
            return Ok(newAccount);
        }
        [HttpPost]
        public ActionResult<Admin> Login(string loginName, string password)
        {
            Admin existingAccount = service.FindByLoginName(loginName);

            if (existingAccount == null)
            {
                return NotFound("Login name not found");
            }

            if(!existingAccount.LoginPassword.Equals(password))
            {
                return BadRequest("Password incorrect");
            }

            return Ok(existingAccount);
        }

        [HttpGet]
        public ActionResult<List<User>> GetUsers(int from, int to)
        {
            if(to < from)
            {
                return BadRequest("Input incorrect!");
            }

            List<User> result = service.GetUsers(from, to);
            return Ok(result);
        }

        [HttpPost]
        public ActionResult SetSuspendUser(string userID, bool suspend)
        {
            bool updated = service.SetSuspendUser(userID, suspend);

            if(!updated)
            {
                return BadRequest("User suspend not updated");
            }

            return Ok($"User suspend was be changed to: {suspend}");
        }

        [HttpDelete]
        public ActionResult DeleteUser(string userID)
        {
            var delteResult = service.DeleteUser(userID);

            if(delteResult.DeletedCount <= 0)
            {
                return BadRequest("User not found");
            }

            return Ok("User was be deleted");
        }
    }
}
