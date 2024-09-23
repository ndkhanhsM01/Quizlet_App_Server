using MongoDB.Driver;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Services
{
    public class AdminService
    {
        protected readonly IMongoCollection<Admin> admin_collection;
        protected readonly IMongoCollection<User> user_collection;
        protected readonly IMongoClient client;
        private readonly IConfiguration config;
        public AdminService(IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(VariableConfig.DatabaseName);
            admin_collection = database.GetCollection<Admin>(VariableConfig.Collection_Admin);
            user_collection = database.GetCollection<User>(VariableConfig.Collection_Users);

            this.client = mongoClient;
            this.config = config;
        }

        public Admin FindByLoginName(string loginName)
        {
            var filter = Builders<Admin>.Filter.Eq(x => x.LoginName, loginName);
            var existingAccount = admin_collection.Find(filter).FirstOrDefault();

            return existingAccount;
        }

        public List<User> GetUsers(int from, int to)
        {
            if (to < from) return null;

            List<User> users = user_collection.Find(user => true)
                                    .Skip(from)
                                    .Limit(to - from)
                                    .ToList();

            return users;
        }

        public bool SetSuspendUser(string userID, bool suspend)
        {
            User user = user_collection.Find(u => u.Id == userID).First();

            if(user == null) return false;

            var update = Builders<User>.Update.Set("is_suspend", suspend);
            var filter = Builders<User>.Filter.Eq(x => x.Id, user.Id);

            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };

            var result = user_collection.FindOneAndUpdate(filter, update, options);

            return true;
        }
    }
}
