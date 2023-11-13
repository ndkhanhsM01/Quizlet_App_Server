using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Services;
using System.Net;

namespace Quizlet_App_Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CardController : ControllerExtend<User>
    {
        private readonly UserService userService;
        public CardController(UserStoreDatabaseSetting setting, IMongoClient mongoClient, IConfiguration config) 
            : base(setting, mongoClient)
        {
            userService = new(mongoClient, config);
        }
        [AllowAnonymous]
        private UpdateResult UpdateDocumentsUser(User existingUser)
        {
            var update = Builders<User>.Update.Set("documents", existingUser.Documents);
            var filter = Builders<User>.Filter.Eq(x => x.Id, existingUser.Id);
            var result = collection.UpdateOne(filter, update);

            return result;
        }
        [HttpPost("userId")]
        public ActionResult<Documents> Create(string userId, [FromBody] FlashCardDTO cardReq)
        {
            User existingUser = userService.FindById(userId);

            if (existingUser == null)
            {
                return NotFound("User not found");
            }
            FlashCard newCard = new()
            {
                Term = cardReq.Term,
                Definition = cardReq.Definition,
                IsPublic = cardReq.IsPublic
            };

            StudySet setOwner = existingUser.Documents.StudySets.Find(x => x.Id == cardReq.IdSetOwner);
            if(setOwner != null)
            {
                newCard.IdSetOwner = setOwner.Id;
                setOwner.Cards.Add(newCard);
            }
            else
            {
                existingUser.Documents.FlashCards.Add(newCard);
            }
            var result = UpdateDocumentsUser(existingUser);

            return new ActionResult<Documents>(existingUser.Documents);
        }

        [HttpPost("userId")]
        public ActionResult<Documents> Update([FromHeader] string userId, [FromHeader] string cardId, [FromBody] FlashCardDTO newInfo)
        {
            User existingUser = userService.FindById(userId);

            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            // find card in document
            FlashCard cardRequire = existingUser.Documents.FlashCards.Find(x => x.Id == cardId);
            if (cardRequire == null)
            {
                for (int i = 0; i < existingUser.Documents.StudySets.Count; i++)
                {
                    var set = existingUser.Documents.StudySets[i];
                    cardRequire = set.Cards.Find(x => x.Id == cardId);
                    if (cardRequire != null) break;
                }
            }
            // update card
            cardRequire.Term = newInfo.Term;
            cardRequire.Definition = newInfo.Definition;
            cardRequire.IdSetOwner = newInfo.IdSetOwner;
            cardRequire.IsPublic = newInfo.IsPublic;

            // update card
            var result = UpdateDocumentsUser(existingUser);

            return new ActionResult<Documents>(existingUser.Documents);
        }

        [HttpPost("userId")]
        public ActionResult<Documents> Remove([FromHeader] string userId, [FromHeader] string cardId)
        {
            User existingUser = userService.FindById(userId);

            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            // find and remove card in document
            FlashCard cardRequire = existingUser.Documents.FlashCards.Find(x => x.Id == cardId);
            if(cardRequire == null)
            {
                int? indexSet = null;
                for(int i=0; i < existingUser.Documents.StudySets.Count; i++)
                {
                    var set = existingUser.Documents.StudySets[i];
                    cardRequire = set.Cards.Find(x => x.Id == cardId);
                    if (cardRequire != null)
                    {
                        indexSet = i;
                        break;
                    }
                }
                if(indexSet != null) 
                    existingUser.Documents.StudySets[(int) indexSet].Cards.Remove(cardRequire);
            }
            else
            {
                existingUser.Documents.FlashCards.Remove(cardRequire);
            }

            // delete card
            var result = UpdateDocumentsUser(existingUser);

            return new ActionResult<Documents>(existingUser.Documents);
        }
    }
}
