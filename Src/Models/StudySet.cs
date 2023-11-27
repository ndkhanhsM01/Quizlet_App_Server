using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Models
{
    [BsonIgnoreExtraElements]
    public class StudySet
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("name")] public string Name { get; set; } = string.Empty;
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        //[BsonElement("cards")] public List<FlashCard> Cards { get; set; } = new List<FlashCard>();
        [BsonElement("id_folder_owner")] public string IdFolderOwner { get; set; } = string.Empty;
        [BsonElement("is_public")] public bool IsPublic { get; set; } = false;
        [BsonElement("count_term")] public int CountTerm { get; set; } = 0;

        public StudySet() { }
        public StudySet(StudySetDTO dto)
        {
            this.Name = dto.Name;
            this.IdFolderOwner = dto.IdFolderOwner;
            this.IsPublic = dto.IsPublic;

            if(dto.AllNewCards != null && dto.AllNewCards.Count > 0) CountTerm = dto.AllNewCards.Count;
            else CountTerm = 0;
        }
    }

    [System.Serializable]
    public class StudySetDTO
    {
        [BsonElement("name")] public string Name { get; set; } = string.Empty;
        [BsonElement("id_folder_owner")] public string IdFolderOwner { get; set; } = string.Empty;
        [BsonElement("is_public")] public bool IsPublic { get; set; } = false;
        [BsonElement("all_new_cards")] public List<FlashCardDTO> AllNewCards { get; set; } = new List<FlashCardDTO>();
    }
}
