using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Services;

namespace Quizlet_App_Server.Controllers
{
    public class ControllerExtend<T>: ControllerBase
    {
        protected readonly IMongoCollection<T> collection;
        protected readonly IMongoClient client;
        public ControllerExtend(IStoreDatabaseSetting setting, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(setting.DatabaseName);
            collection = database.GetCollection<T>(setting.CollectionName);

            this.client = mongoClient;
        }
    }
}
