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
            return userService.UpdateDocumentsUser(existingUser);
        }
        [HttpPost]
        public ActionResult<Documents> Create(string userId, [FromBody] FlashCardDTO cardReq)
        {
            User existingUser = userService.FindById(userId);

            if (existingUser == null)
            {
                return NotFound("User not found");
            }
            FlashCard newCard = new(cardReq);

            StudySet setOwner = existingUser.Documents.StudySets.Find(x => x.Id == cardReq.IdSetOwner);
            if(setOwner != null)
            {
                newCard.IdSetOwner = setOwner.Id;
            }
            else
            {
                newCard.IdSetOwner = string.Empty;
            }

            existingUser.Documents.FlashCards.Add(newCard);
            var result = UpdateDocumentsUser(existingUser);

            return new ActionResult<Documents>(existingUser.Documents);
        }

        [HttpPut]
        public ActionResult<Documents> Update(string userId, string cardId, [FromBody] FlashCardDTO newInfo)
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
                return NotFound("Card require not found");
            }
            if(existingUser.Documents.StudySets.All(set => !set.Id.Equals(newInfo.IdSetOwner)))
            {
                newInfo.IdSetOwner = string.Empty;
            }
            // update card
            cardRequire.Term = newInfo.Term;
            cardRequire.Definition = newInfo.Definition;
            cardRequire.IdSetOwner = newInfo.IdSetOwner;
            //cardRequire.IsPublic = newInfo.IsPublic;

            // update card
            var result = UpdateDocumentsUser(existingUser);

            return new ActionResult<Documents>(existingUser.Documents);
        }

        [HttpDelete]
        public ActionResult<Documents> Delete(string userId, string cardId)
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
                return NotFound("Card not found");
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
