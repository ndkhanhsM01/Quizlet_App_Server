using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Models.Helper;
using Quizlet_App_Server.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text;

namespace Quizlet_App_Server.Services
{
    public class UserService
    {
        protected readonly IMongoCollection<User> collection;
        protected readonly IMongoClient client;
        private readonly IConfiguration config;
        public UserService(IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(VariableConfig.DatabaseName);
            collection = database.GetCollection<User>(VariableConfig.Collection_Users);

            this.client = mongoClient;
            this.config = config;
        }

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

        public T GetConfigData<T>(string specialName) where T: Configurable
        {
            var database = client.GetDatabase(VariableConfig.DatabaseName);
            var configCollection = database.GetCollection<T>(VariableConfig.Collection_Configure);

            var filter = Builders<T>.Filter.Eq(x => x.SpecialName, specialName);
            var existingDocument = configCollection.Find(filter).FirstOrDefault();

            return existingDocument;
        }
        public User FindBySeqId(int seqId)
        {
            var filter = Builders<User>.Filter.Eq(x => x.SeqId, seqId);
            var existingUser = collection.Find(filter).FirstOrDefault();

            return existingUser;
        }
        public User FindById(string id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var existingUser = collection.Find(filter).FirstOrDefault();

            return existingUser;
        }
        public User FindByLoginName(string loginName)
        {
            var filter = Builders<User>.Filter.Eq(x => x.LoginName, loginName);
            var existingUser = collection.Find(filter).FirstOrDefault();

            return existingUser;
        }
        public UpdateResult UpdateDocumentsUser(User existingUser)
        {
            var update = Builders<User>.Update.Set("documents", existingUser.Documents);
            var filter = Builders<User>.Filter.Eq(x => x.Id, existingUser.Id);
            var result = collection.UpdateOne(filter, update);

            return result;
        }
        public UpdateResult UpdateCollectionStorage(User existingUser)
        {
            var update = Builders<User>.Update.Set("collection_storage", existingUser.CollectionStorage);
            var filter = Builders<User>.Filter.Eq(x => x.Id, existingUser.Id);
            var result = collection.UpdateOne(filter, update);

            return result;
        }
        public InfoPersonal UpdateInfoUser(string userId, InfoPersonal newInfo)
        {
            var updateDefinitionList = new List<UpdateDefinition<User>>();

            if (!newInfo.UserName.IsNullOrEmpty())
            {
                updateDefinitionList.Add(Builders<User>.Update.Set("user_name", newInfo.UserName));
            }
            if (!newInfo.Email.IsNullOrEmpty())
            {
                updateDefinitionList.Add(Builders<User>.Update.Set("email", newInfo.Email));
            }
/*            if (newInfo.Avatar != null)
            {
                updateDefinitionList.Add(Builders<User>.Update.Set("avatar", newInfo.Avatar));
            }*/
            if (!newInfo.DateOfBirth.IsNullOrEmpty())
            {
                updateDefinitionList.Add(Builders<User>.Update.Set("date_of_birth", newInfo.DateOfBirth));
            }
            if (newInfo.Setting != null)
            {
                updateDefinitionList.Add(Builders<User>.Update.Set("setting", newInfo.Setting));
            }

            var combinedUpdate = Builders<User>.Update.Combine(updateDefinitionList);



            var filter = Builders<User>.Filter.Eq(x => x.Id, userId);

            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After 
            };

            var updatedUser = collection.FindOneAndUpdate(filter, combinedUpdate, options);
            return updatedUser.GetInfo();
        }
        public User UpdateAchievement(string userId, Achievement newAchievement)
        {
            var update = Builders<User>.Update.Set("achievement", newAchievement);
            var filter = Builders<User>.Filter.Eq(x => x.Id, userId);

            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updatedUser = collection.FindOneAndUpdate(filter, update, options);
            return updatedUser;
        }
        // To generate token
        //public string GenerateToken(User user)
        //{
        //    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        //    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        //    var claims = new[]
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, user.LoginName),
        //    };
        //    var token = new JwtSecurityToken(config["Jwt:Issuer"],
        //        config["Jwt:Audience"],
        //        claims,
        //        expires: DateTime.Now.AddMinutes(15),
        //        signingCredentials: credentials);


        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}
    }
}
