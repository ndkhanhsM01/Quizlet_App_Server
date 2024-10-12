using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Src.Services;
using Quizlet_App_Server.Utility;

namespace Tetris
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TetrisUserController: ControllerBase
    {
        protected readonly TetrisUserService service;
        protected readonly IMongoCollection<UserScore> collection;
        protected readonly IMongoClient client;
        public TetrisUserController(IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(VariableConfig.Tetris_DatabaseName);
            collection = database.GetCollection<UserScore>(VariableConfig.Collection_TetrisScore);

            this.client = mongoClient;
            this.service = new(mongoClient, config);
        }

        [HttpPost]
        public ActionResult<UserScore> CreateNewUser(string username)
        {
            UserScore newUser = service.CreateNewUser(username);

            return new ActionResult<UserScore>(newUser);
        }

        [HttpPost]
        public ActionResult<Dictionary<string, object>> UpdateScore(long userId, int newScore)
        {
            UserScore newData = service.UpdateValue(userId, "score", newScore);

            if(newData == null)
            {
                return BadRequest($"User not found <{userId}>");
            }

            Dictionary<string, object> result = new();
            result.Add("ranking", service.GetRanking(userId));
            result.Add("top10", service.GetTopUserScore(10));

            return new ActionResult<Dictionary<string, object>>(result);
        }

        [HttpPost]
        public ActionResult<UserScore> UpdateName(long userId, string newName)
        {
            UserScore newData = service.UpdateValue(userId, "name", newName);

            if (newData == null)
            {
                return BadRequest($"User not found <{userId}>");
            }

            return new ActionResult<UserScore>(newData);
        }

        [HttpGet]
        public ActionResult<Dictionary<string, object>> GetRankingResult(long userId)
        {
            UserScore existUser = collection.Find(u => u.SeqID == userId).First();

            if(existUser == null)
            {
                return BadRequest($"Not found user's seq_id {userId}");
            }

            Dictionary<string, object> result = new();
            result.Add("ranking", service.GetRanking(userId));
            result.Add("top10", service.GetTopUserScore(10));

            return new ActionResult<Dictionary<string, object>>(result);
        }
    }
}
