﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
        public ActionResult<UserRespone> UpdateInfo(string userId, string folderId, [FromBody] FolderDTO req)
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
            existFolder.Description = req.Description;
            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
        [HttpPost]
        public ActionResult<UserRespone> InsertNewSet(string userId, string folderId, [FromBody] List<StudySetDTO> newSets)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            Folder folder = userExisting.Documents.Folders.Find(f => f.Id.Equals(folderId));
            if (folder == null)
            {
                return BadRequest("Not found folder in user's document");
            }

            foreach(var s in newSets)
            {
                folder.AddNewSet(s);
            }

            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
        [HttpPost]
        public ActionResult<UserRespone> InsertSetExisting(string userId, string folderId, [FromBody] List<string> idSetExisting)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            Folder folder = userExisting.Documents.Folders.Find(f => f.Id.Equals(folderId));
            if (folder == null)
            {
                return BadRequest("Not found folder in user's document");
            }

            var allSet = userExisting.Documents.GetAllSets();
            int countAdded = 0;
            foreach (var set in allSet)
            {

                if (idSetExisting.Contains(set.Id))
                {
                    if (set.IdFolderOwner.IsNullOrEmpty())
                    {
                        userExisting.Documents.StudySets.Remove(set);
                    }
                    folder.AddNewSet(set);
                    countAdded++;
                }

                if (countAdded == idSetExisting.Count) break;
            }

            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
        [HttpDelete]
        public ActionResult<UserRespone> RemoveSet(string userId, string folderId, string setId)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            Folder folder = userExisting.Documents.Folders.Find(f => f.Id.Equals(folderId));
            if (folder == null)
            {
                return BadRequest("Not found folder in user's document");
            }

            int removed = folder.StudySets.RemoveAll(s => s.Id.Equals(setId));
            if (removed <= 0)
            {
                return BadRequest("Not found set");
            };

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

            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
    }
}
