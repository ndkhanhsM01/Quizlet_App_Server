﻿using Microsoft.AspNetCore.Mvc;
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
        public ActionResult<Documents> Create(string userId, [FromBody] StudySetDTO req)
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

            StudySet newSet = new()
            {
                Name = req.Name,
                IdFolderOwner = req.IdFolderOwner
            };
            userExisting.Documents.StudySets.Add(newSet);
            userService.UpdateDocumentsUser(userExisting);
        
            return new ActionResult<Documents>(userExisting.Documents);
        }

        [HttpPut("userId")]
        public ActionResult<Documents> Update(string userId, [FromHeader] string setId, [FromBody] StudySetDTO req)
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
                    setFound = true;
                    break;
                }
            }

            // update set if was found
            if (setFound)
            {
                userService.UpdateDocumentsUser(userExisting);

                return new ActionResult<Documents>(userExisting.Documents);
            }
            else
            {
                return BadRequest("Set's Id inconrrect");
            }
        }
        [HttpPut("userId")]
        public ActionResult<Documents> Delete(string userId, [FromHeader] string setId)
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

            return new ActionResult<Documents>(userExisting.Documents);
        }
    }
}