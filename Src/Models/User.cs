using Microsoft.AspNetCore.Mvc.ViewEngines;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Libmongocrypt;
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
        //[BsonElement("avatar")] public string Avatar { get; set; } = string.Empty;
        [BsonElement("date_of_birth")] public string DateOfBirth { get; set; } = "1999-01-01";
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("collection_storage")] public UserCollection CollectionStorage { get; set; } = new UserCollection();
        [BsonElement("documents")] public Documents Documents { get; set; } = new Documents();
        [BsonElement("streak")] public Streak Streak { get; set; } = new Streak();
        [BsonElement("achievement")] public Achievement Achievement { get; set; } = new Achievement();
        [BsonElement("setting")] public UserSetting Setting { get; set; } = new UserSetting();
        //[BsonElement("avatar")] public List<int> Avatar { get; set; } = new List<int>();

        public void UpdateInfo(InfoPersonal newInfo)
        {
            this.UserName = newInfo.UserName;
            this.Email = newInfo.Email;
            //this.Avatar = newInfo.Avatar;
            this.DateOfBirth = newInfo.DateOfBirth;
            this.Setting = newInfo.Setting;
        }

        public void UpdateStreak()
        {
            if (Streak == null) return;

            Streak.CurrentStreak++;

            foreach(var task in Achievement.TaskList)
            {
                if (!task.Type.Equals(TaskType.STREAK)) continue;

                if (task.Progress >= task.Condition) continue;

                bool wasCompleted = task.Status >= TaskStatus.Completed;
                task.Progress = Streak.CurrentStreak;

                if(!wasCompleted && task.Status >= TaskStatus.Completed) 
                    this.CollectionStorage.Score += task.Score ?? 0;
            }
        }
        public void UpdateScore(int value)
        {
            this.CollectionStorage.Score += value;
        }
        public void UpdateScore(int baseValue, int multiple)
        {
            int value = baseValue * multiple;
            this.CollectionStorage.Score += value;
        }
        public InfoPersonal GetInfo()
        {
            return new InfoPersonal()
            {
                UserName = this.UserName,
                Email = this.Email,
                //Avatar = this.Avatar,
                DateOfBirth = this.DateOfBirth,
                Setting = this.Setting
            };
        }
        public InforUserRanking GetInfoScore()
        {
            return new InforUserRanking()
            {
                Score = this.CollectionStorage.Score,
                SeqId = this.SeqId,
                UserName = this.UserName,
                Email = this.Email,
                //Avatar = this.Avatar,
                DateOfBirth = this.DateOfBirth
            };
        }
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

    [System.Serializable]
    public class UserRespone
    {
        public string Id { get; set; } = string.Empty;
        [BsonElement("seq_id")] public int SeqId { get; set; }
        [BsonElement("login_name")] public string LoginName { get; set; } = string.Empty;
        //[BsonElement("login_password")] public string LoginPassword { get; set; } = string.Empty;
        [BsonElement("user_name")] public string UserName { get; set; } = string.Empty;
        [BsonElement("email")] public string Email { get; set; } = string.Empty;
        //[BsonElement("avatar")] public string Avatar { get; set; } = string.Empty;
        [BsonElement("date_of_birth")] public string DateOfBirth { get; set; } = "1999-01-01";
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("documents")] public Documents Documents { get; set; } = new Documents();
        [BsonElement("setting")] public UserSetting Setting { get; set; } = new UserSetting();
        //[BsonElement("avatar")] public List<int> Avatar { get; set; } = new List<int>();

        public UserRespone(User user)
        {
            this.Id = user.Id;
            this.SeqId = user.SeqId;
            this.LoginName = user.LoginName;
            this.UserName = user.UserName;
            this.Email = user.Email;
            //this.Avatar = user.Avatar;
            this.DateOfBirth = user.DateOfBirth;
            this.TimeCreated = user.TimeCreated;
            this.Documents = user.Documents;
            this.Setting = user.Setting;
        }
    }

    [System.Serializable]
    public class InfoPersonal
    {
        [BsonElement("user_name")] public string? UserName { get; set; } = string.Empty;
        [BsonElement("email")] public string? Email { get; set; } = string.Empty;
        //[BsonElement("avatar")] public string Avatar { get; set; } = string.Empty;
        [BsonElement("date_of_birth")] public string? DateOfBirth { get; set; } = string.Empty;
        [BsonElement("setting")] public UserSetting? Setting { get; set; } = new UserSetting();
        //[BsonElement("avatar")] public List<int>? Avatar { get; set; } = new List<int>();
    }

    [System.Serializable]
    public class InforUserRanking
    {
        [BsonElement("score")] public int Score { get; set; } = 0;
        [BsonElement("seq_id")] public int SeqId { get; set; } = 0;
        [BsonElement("user_name")] public string? UserName { get; set; } = string.Empty;
        [BsonElement("email")] public string? Email { get; set; } = string.Empty;
        //[BsonElement("avatar")] public string Avatar { get; set; } = string.Empty;
        [BsonElement("date_of_birth")] public string? DateOfBirth { get; set; } = string.Empty;
        //[BsonElement("avatar")] public List<int>? Avatar { get; set; } = new List<int>();
    }

    [System.Serializable]
    public class UserCollection
    {
        [BsonElement("create_set_count")] public int CreateSetCount { get; set; } = 0;
        [BsonElement("score")] public int Score { get; set; } = 0;
    }
}
