using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Models
{
    [BsonIgnoreExtraElements]
    public class FlashCard
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("term")] public string Term { get; set; } = string.Empty;
        [BsonElement("definition")] public string Definition { get; set; } = string.Empty;
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("is_public")] public bool IsPublic { get; set; } = false;
        [BsonElement("id_set_owner")] public string IdSetOwner { get; set; } = string.Empty;
    }

    [Serializable]
    public class FlashCardDTO
    {
        [BsonElement("term")] public string Term { get; set; } = string.Empty;
        [BsonElement("definition")] public string Definition { get; set; } = string.Empty;
        [BsonElement("id_set_owner")] public string IdSetOwner { get; set; } = string.Empty;
        [BsonElement("is_public")] public bool IsPublic { get; set; } = false;
    }
}
