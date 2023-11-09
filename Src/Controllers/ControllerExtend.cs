using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;

namespace Quizlet_App_Server.Controllers
{
    public class ControllerExtend<T>: ControllerBase
    {
        protected readonly IMongoCollection<T> collection;
        public ControllerExtend(IStoreDatabaseSetting setting, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(setting.DatabaseName);
            collection = database.GetCollection<T>(setting.CollectionName);
        }
    }
}
