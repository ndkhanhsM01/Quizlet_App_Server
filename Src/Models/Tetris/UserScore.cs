

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quizlet_App_Server.Utility;

namespace Tetris
{
    public class UserScore
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("seq_id")] public long SeqID { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("name")] public string Name { get; set; } = string.Empty;
        [BsonElement("score")] public int Score { get; set; }
    }
}
