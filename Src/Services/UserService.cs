using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Models.Helper;
using Quizlet_App_Server.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        public List<FlashCard> FindAllCardsSameSet(string idUser, string idSet)
        {
            User user = FindById(idUser);
            if(user == null)
            {
                return null;
            }

            List<FlashCard> allCards = user.Documents.FlashCards;
            List<FlashCard> listRespone = new List<FlashCard>();
            foreach(var card in allCards)
            {
                if (card.IdSetOwner.Equals(idSet))
                    listRespone.Add(card);
            }

            return listRespone;
        }
        public List<StudySet> FindAllSetsSameFolder(string idUser, string idFolder)
        {
            User user = FindById(idUser);
            if (user == null)
            {
                return null;
            }

            List<StudySet> allSets = user.Documents.StudySets;
            List<StudySet> listRespone = new List<StudySet>();
            foreach (var set in allSets)
            {
                if (set.IdFolderOwner.Equals(idFolder))
                    listRespone.Add(set);
            }

            return listRespone;
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
