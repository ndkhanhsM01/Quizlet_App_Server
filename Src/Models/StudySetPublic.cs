using MongoDB.Bson.Serialization.Attributes;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Models
{
    public class StudySetPublic
    {
        [BsonElement("id_owner")] public string id_owner { get; set;} = string.Empty;
        [BsonElement("name")] public string Name { get; set; } = string.Empty;
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;
        //[BsonElement("id_folder_owner")] public string IdFolderOwner { get; set; } = string.Empty;
        //[BsonElement("is_public")] public bool IsPublic { get; set; } = false;
        [BsonElement("all_cards")] public List<FlashCard> AllCards { get; set; } = new List<FlashCard>();
        public StudySetPublic(string idOwner, StudySet set, Documents documents) 
        {
            List<FlashCard> allCardOfSet = documents.FlashCards.FindAll(card => card.IdSetOwner.Equals(set.Id));

            if (allCardOfSet != null)
            {
                this.id_owner = idOwner;
                this.Name = set.Name;
                this.TimeCreated = set.TimeCreated;
                this.AllCards = allCardOfSet;
            }
        }
    }
}
