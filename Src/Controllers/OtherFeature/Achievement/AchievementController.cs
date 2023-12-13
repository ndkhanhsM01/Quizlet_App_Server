using Microsoft.AspNetCore.Mvc;
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
    public class AchievementController : ControllerBase
    {
        protected readonly IMongoCollection<Achievement> collection;
        protected readonly IMongoClient client;
        public AchievementController(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(VariableConfig.DatabaseName);
            collection = database.GetCollection<Achievement>(VariableConfig.Collection_Configure);

            this.client = mongoClient;
        }
        [HttpPost]
        public ActionResult WriteConfig([FromBody] List<Models.Task> listTask)
        {
            string nameConfig = "Achievement";
            var filter = Builders<Achievement>.Filter.Eq(x => x.SpecialName, nameConfig);
            var existingDocument = collection.Find(filter).FirstOrDefault();

            if(existingDocument == null)
            {
                Achievement achievement = new Achievement();
                achievement.TaskList = listTask;
                collection.InsertOne(achievement);
                return Ok("Create new Achievement config success!");
            }
            else
            {
                existingDocument.TaskList.AddRange(listTask);
                var update = Builders<Achievement>.Update.Set("task_list", existingDocument.TaskList);
                collection.UpdateOne(filter, update);
                return Ok("Update config success!");
            }
        }
    }
}
