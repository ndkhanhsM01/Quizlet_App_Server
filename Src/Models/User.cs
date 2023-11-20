using Microsoft.AspNetCore.Mvc.ViewEngines;
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
        [BsonElement("seq_id")] public int SeqId { get; set; }
        [BsonElement("login_name")] public string LoginName { get; set; } = string.Empty;
        [BsonElement("login_password")] public string LoginPassword { get; set; } = string.Empty;
        [BsonElement("user_name")] public string UserName { get; set; } = string.Empty;
        [BsonElement("email")] public string Email { get; set; } = string.Empty;
        [BsonElement("date_of_birth")] public string DateOfBirth { get; set; } = "1999-01-01";
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("documents")] public Documents Documents { get; set; } = new Documents();
        [BsonElement("setting")] public UserSetting Setting { get; set; } = new UserSetting();
    }

    [System.Serializable]
    public class UserSignUp
    {
        public string LoginName { get; set; } = string.Empty;
        public string LoginPassword { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = "1999-01-01";
    }

    [System.Serializable]
    public class UserLogin
    {
        [BsonElement("login_name")] public string LoginName { get; set; } = string.Empty;
        [BsonElement("login_password")] public string LoginPassword { get; set; } = string.Empty;
    }

    [System.Serializable]
    public class ChangePasswordRequest
    {
        [BsonElement("old_password")] public string OldPassword { get; set; } = string.Empty;
        [BsonElement("new_password")] public string NewPassword { get; set; } = string.Empty;
    }
}
