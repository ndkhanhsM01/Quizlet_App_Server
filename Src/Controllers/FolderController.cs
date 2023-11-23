using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.Controllers;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Services;

namespace Quizlet_App_Server.Src.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FolderController : ControllerExtend<User>
    {
        protected readonly UserService userService;
        public FolderController(UserStoreDatabaseSetting setting, IMongoClient mongoClient, IConfiguration config)
            : base(setting, mongoClient)
        {
            userService = new(mongoClient, config);
        }
        [HttpPost]
        public ActionResult<UserRespone> Create(string userId, [FromBody] FolderDTO req)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }
            else if(userExisting.Documents.Folders.Any(folder => folder.Name.Equals(req.Name)))
            {
                return BadRequest("Has exist other folder same name");
            }

            Folder newFolder = new(req);
            userExisting.Documents.Folders.Add(newFolder);
            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
        [HttpPut]
        public ActionResult<UserRespone> Update(string userId, string folderId, [FromBody] FolderDTO req)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }
            else if(userExisting.Documents.Folders.Count <= 0 
                || !userExisting.Documents.Folders.Any(folder => folder.Id.Equals(folderId)))
            {
                return BadRequest("Not found folder in user's document");
            }

            // update folder
            Folder existFolder = userExisting.Documents.Folders.Find(folder => folder.Id == folderId);
            existFolder.Name = req.Name;
            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }

        [HttpDelete]
        public ActionResult<UserRespone> Delete(string userId, string folderId)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }
            else if (userExisting.Documents.Folders.Count <= 0
                || !userExisting.Documents.Folders.Any(folder => folder.Id.Equals(folderId)))
            {
                return BadRequest("Not found folder in user's document");
            }

            // remove folder
            userExisting.Documents.Folders.RemoveAll(folder => folder.Id.Equals(folderId));

            List<string> setRemoved = new();
            foreach (var set in userExisting.Documents.StudySets)
            {
                if (set.IdFolderOwner == folderId)
                    setRemoved.Add(set.Id);
            }
            // remove sets and cards are related to each other
            userExisting.Documents.FlashCards.RemoveAll(card => setRemoved.Contains(card.IdSetOwner));
            userExisting.Documents.StudySets.RemoveAll(set => set.IdFolderOwner.Equals(folderId));
            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
    }
}
