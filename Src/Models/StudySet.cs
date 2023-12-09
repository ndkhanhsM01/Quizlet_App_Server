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
        [BsonElement("id_folder_owner")] public string IdFolderOwner { get; set; } = string.Empty;
        [BsonElement("is_public")] public bool IsPublic { get; set; } = false;
        [BsonElement("description")] public string Description { get; set; } = string.Empty;
        [BsonElement("count_term")] public int CountTerm
        {
            get
            {
                if (Cards == null) return 0;
                else return Cards.Count;
            }
        }
        [BsonElement("cards")] public List<FlashCard> Cards { get; set; } = new List<FlashCard>();
        public StudySet() { }
        public StudySet(StudySetDTO dto)
        {
            this.Name = dto.Name;
            this.IdFolderOwner = dto.IdFolderOwner;
            this.IsPublic = dto.IsPublic;
            this.Description = dto.Description;
            if (dto.AllNewCards != null && dto.AllNewCards.Count > 0)
            {
                this.Cards = new();
                foreach (FlashCardDTO cardDTO in dto.AllNewCards)
                {
                    cardDTO.IdSetOwner = this.Id;
                    Cards.Add(new FlashCard(cardDTO));
                }
            }
        }
        public void AddNewCard(FlashCardDTO cardDTO)
        {
            cardDTO.IdSetOwner = this.Id;
            Cards.Add(new FlashCard(cardDTO));
        }
        public void UpdateInfo(StudySetDTO dto)
        {
            this.Id = this.Id;
            this.TimeCreated = this.TimeCreated;
            this.Name = dto.Name;
            this.IdFolderOwner = dto.IdFolderOwner;
            this.IsPublic = dto.IsPublic;
            this.Description = dto.Description;
        }
    }

    [System.Serializable]
    public class StudySetDTO
    {
        public string Id { get; set; } = string.Empty;
        [BsonElement("name")] public string Name { get; set; } = string.Empty;
        [BsonElement("id_folder_owner")] public string IdFolderOwner { get; set; } = string.Empty;
        [BsonElement("is_public")] public bool IsPublic { get; set; } = false;
        [BsonElement("description")] public string Description { get; set; } = string.Empty;
        [BsonElement("all_new_cards")] public List<FlashCardDTO> AllNewCards { get; set; } = new List<FlashCardDTO>();
    }
}
