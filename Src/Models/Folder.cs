using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Models
{
    [BsonIgnoreExtraElements]
    public class Folder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        [BsonElement("name")] public string Name { get; set; } = string.Empty;
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        //[BsonElement("study_sets")] public List<StudySet> StudySets { get; set; } = new List<StudySet>();
        [BsonElement("description")] public string Description { get; set; } = string.Empty;
        public Folder() { }
        public Folder(FolderDTO dto)
        {
            this.Name = dto.Name;
            this.Description = dto.Description;
        }
    }

    [System.Serializable]
    public class FolderDTO
    {
        [BsonElement("name")] public string Name { get; set; } = string.Empty;
        [BsonElement("description")] public string Description { get; set; } = string.Empty;
    }
}
