using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Utility;

namespace Tetris
{
    public class TetrisUserService
    {
        protected readonly IMongoCollection<UserScore> userScore_collection;
        protected readonly IMongoClient client;
        private readonly IConfiguration config;
        public TetrisUserService(IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(VariableConfig.Tetris_DatabaseName);
            userScore_collection = database.GetCollection<UserScore>(VariableConfig.Collection_TetrisScore);

            this.client = mongoClient;
            this.config = config;
        }

        public UserScore CreateNewUser(string userName)
        {
            UserScore newUser = new UserScore();
            newUser.Name = userName;

            userScore_collection.InsertOne(newUser);
            return newUser;
        }

        public UserScore UpdateValue(long userId, string nameField, object value)
        {
            UserScore user = userScore_collection.Find(u => u.SeqID == userId).First();

            if (user == null) return null;

            var update = Builders<UserScore>.Update.Set(nameField, value);
            var filter = Builders<UserScore>.Filter.Eq(x => x.Id, user.Id);

            var options = new FindOneAndUpdateOptions<UserScore>
            {
                ReturnDocument = ReturnDocument.After
            };

            var result = userScore_collection.FindOneAndUpdate(filter, update, options);

            return result;
        }

        public List<UserScore> GetTopUserScore(int amount = 10)
        {
            List<UserScore> result = new List<UserScore>();

            var filter = Builders<UserScore>.Filter.Empty;
            var sort = Builders<UserScore>.Sort.Descending(u => u.Score);

            result = userScore_collection.Find(filter)
                                          .Sort(sort)
                                          .Limit(amount)
                                          .ToList();

            return result;
        }

        public int GetRanking(long id)
        {
            UserScore user = userScore_collection.Find(u => u.SeqID == id).First();
            long ranking = userScore_collection.CountDocuments(u => u.Score > user.Score);
            return (int) ranking;
        }
    }
}
