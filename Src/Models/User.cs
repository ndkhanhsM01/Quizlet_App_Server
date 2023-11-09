using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [BsonElement("login_name")] public string LoginName { get; set; } = string.Empty;
        [BsonElement("login_password")] public string LoginPassword { get; set; } = string.Empty;
        [BsonElement("user_id")] public int UserId { get; set; }
        [BsonElement("user_name")] public string UserName { get; set; } = string.Empty;
        [BsonElement("email")] public string Email { get; set; } = string.Empty;
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("documents")] public Documents Documents { get; set; } = new Documents();
        [BsonElement("setting")] public UserSetting Setting { get; set; } = new UserSetting();
    }

    [System.Serializable]
    public class UserRequest
    {
        public string LoginName { get; set; } = string.Empty;
        public string LoginPassword { get; set; } = string.Empty;
    }
}
