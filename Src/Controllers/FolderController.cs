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
        public ActionResult<Documents> Create(string userId, [FromBody] FolderDTO req)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            Folder newFolder = new(req);
            userExisting.Documents.Folders.Add(newFolder);
            userService.UpdateDocumentsUser(userExisting);

            return new ActionResult<Documents>(userExisting.Documents);
        }
        [HttpPut]
        public ActionResult<Documents> Update(string userId, string folderId, [FromBody] FolderDTO req)
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

            return new ActionResult<Documents>(userExisting.Documents);
        }

        [HttpDelete]
        public ActionResult<Documents> Delete(string userId, string folderId)
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
            // remove study set
            //StudySetController studySetController = new(setting, )
            //userService.UpdateDocumentsUser(userExisting);

            return new ActionResult<Documents>(userExisting.Documents);
        }
    }
}
