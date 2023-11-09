using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Quizlet_App_Server.Models
{
    [BsonIgnoreExtraElements]
    public class Documents
    {
        [BsonElement("folders")] public List<Folder> Folders { get; set; } = new List<Folder>();
        [BsonElement("study_sets")] public List<StudySet> StudySets { get; set; } = new List<StudySet>();
        [BsonElement("flash_cards")] public List<FlashCard> FlashCards { get; set; } = new List<FlashCard>();
    }
}
