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
        [BsonElement("description")] public string Description { get; set; } = string.Empty;
        [BsonElement("count_sets")]
        public int CountSets
        {
            get
            {
                if (StudySets == null) return 0;
                else return StudySets.Count;
            }
        }
        [BsonElement("study_sets")] public List<StudySet> StudySets { get; set; } = new List<StudySet>();

        public Folder() { }
        public Folder(FolderDTO dto)
        {
            this.Name = dto.Name;
            this.Description = dto.Description;
            this.StudySets = dto.StudySets;
        }
        public void AddNewSet(StudySetDTO newSet)
        {
            newSet.IdFolderOwner = this.Id;
            StudySets.Add(new StudySet(newSet));
        }
        public void AddNewSet(StudySet newSet)
        {
            newSet.IdFolderOwner = this.Id;
            StudySets.Add(newSet);
        }
    }

    [System.Serializable]
    public class FolderDTO
    {
        [BsonElement("name")] public string Name { get; set; } = string.Empty;
        [BsonElement("description")] public string Description { get; set; } = string.Empty;
        [BsonElement("study_sets")] public List<StudySet> StudySets { get; set; } = new List<StudySet>();
    }
}
