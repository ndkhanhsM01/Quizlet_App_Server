using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Services;

namespace Quizlet_App_Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StudySetController : ControllerExtend<User>
    {
        protected readonly UserService userService;
        public StudySetController(UserStoreDatabaseSetting setting, IMongoClient mongoClient, IConfiguration config) 
            : base(setting, mongoClient)
        {
            userService = new(mongoClient, config);
        }

        [HttpPost]
        public ActionResult<UserRespone> Create(string userId, [FromBody] StudySetDTO req)
        {
            User userExisting = userService.FindById(userId);
            if(userExisting == null)
            {
                return NotFound("User not found");
            }

            Folder folderOwner = userExisting.Documents.Folders.Find(x => x.Id.Equals(req.IdFolderOwner));
            if(folderOwner == null)
            {
                req.IdFolderOwner = string.Empty;
            }
            if (userExisting.Documents.StudySets.Any(set => set.Name.Equals(req.Name) && set.IdFolderOwner.Equals(req.IdFolderOwner)))
            {
                return BadRequest("Has exist other study set same name in folder");
            }

            StudySet newSet = new(req);
            List<FlashCard> allNewCards = new List<FlashCard>();
            if(req.AllNewCards != null && req.AllNewCards.Count > 0)
            {
                foreach(FlashCardDTO cardDTO in req.AllNewCards)
                {
                    cardDTO.IdSetOwner = newSet.Id;
                    allNewCards.Add(new FlashCard(cardDTO));
                }
            }

            userExisting.Documents.StudySets.Add(newSet);
            if(allNewCards.Count > 0) userExisting.Documents.FlashCards.AddRange(allNewCards);
            userService.UpdateDocumentsUser(userExisting);
        
            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }

        [HttpPut]
        public ActionResult<UserRespone> Update(string userId, string setId, [FromBody] StudySetDTO req)
        {
            User userExisting = userService.FindById(userId);
            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            bool setFound = false;
            foreach(StudySet set in userExisting.Documents.StudySets)
            {
                if(set.Id.Equals(setId))
                {
                    set.IdFolderOwner = req.IdFolderOwner;
                    set.Name = req.Name;
                    set.IsPublic = req.IsPublic;
                    setFound = true;
                    break;
                }
            }

            // update set if was found
            if (setFound)
            {
                userService.UpdateDocumentsUser(userExisting);

                UserRespone respone = new UserRespone(userExisting);
                return new ActionResult<UserRespone>(respone);
            }
            else
            {
                return BadRequest("Set's Id inconrrect");
            }
        }
        [HttpDelete]
        public ActionResult<UserRespone> Delete(string userId, string setId)
        {
            User userExisting = userService.FindById(userId);
            if (userExisting == null)
            {
                return NotFound("User not found");
            }
            else if(userExisting.Documents.StudySets.Count <= 0 || !userExisting.Documents.StudySets.Any(set => set.Id.Equals(setId)))
            {
                return BadRequest("Set's Id inconrrect");
            }

            // delete set require
            
            userExisting.Documents.StudySets.RemoveAll(set => set.Id.Equals(setId));
            userExisting.Documents.FlashCards.RemoveAll(card => card.IdSetOwner.Equals(setId));

            // update documents
            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
    }
}
