using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
        public ActionResult<UserRespone> Create(string userId, [FromBody] FlashCardDTO cardReq)
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

            if (setOwner != null && setOwner.Id.Equals(cardReq.IdSetOwner))
            {
                setOwner.Cards.Add(newCard);
            }
            else
            {
                newCard.IdSetOwner = string.Empty;
                existingUser.Documents.FlashCards.Add(newCard);
            }

            var result = UpdateDocumentsUser(existingUser);

            UserRespone respone = new UserRespone(existingUser);
            return new ActionResult<UserRespone>(respone);
        }

        [HttpPut]
        public ActionResult<UserRespone> UpdateInfo(string userId, string cardId, [FromBody] FlashCardDTO req)
        {
            User existingUser = userService.FindById(userId);

            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            // find card in document
            FlashCard cardRequire = existingUser.Documents.GetAllCards().Find(x => x.Id == cardId);

            if (cardRequire == null)
            {
                return BadRequest("card not found");
            }

            // update card
            cardRequire.Term = req.Term;
            cardRequire.Definition = req.Definition;


            // update card
            var result = UpdateDocumentsUser(existingUser);

            UserRespone respone = new UserRespone(existingUser);
            return new ActionResult<UserRespone>(respone);
        }

        [HttpDelete]
        public ActionResult<UserRespone> Delete(string userId, string cardId)
        {
            User existingUser = userService.FindById(userId);

            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            // find and remove card in document
            FlashCard cardRequire = existingUser.Documents.GetAllCards().Find(card => card.Id.Equals(cardId));
            
            if(cardRequire == null)
            {
                return NotFound("Not found card");
            }

            if (cardRequire.IdSetOwner.IsNullOrEmpty())
            {
                existingUser.Documents.FlashCards.Remove(cardRequire);
            }
            else
            {
                StudySet setOwner = existingUser.Documents.GetAllSets().Find(set => set.Id.Equals(cardRequire.IdSetOwner));
                setOwner?.Cards.Remove(cardRequire);
            }

            // delete card
            var result = UpdateDocumentsUser(existingUser);

            UserRespone respone = new UserRespone(existingUser);
            return new ActionResult<UserRespone>(respone);
        }
    }
}
