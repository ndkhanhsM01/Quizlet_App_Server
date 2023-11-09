﻿using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Quizlet_App_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlashCardController : ControllerExtend<FlashCard>
    {
        public FlashCardController(IStoreDatabaseSetting setting, IMongoClient mongoClient) 
            : base(setting, mongoClient)
        {
        }

        // GET: api/<FlashCardController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<FlashCardController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<FlashCardController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<FlashCardController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<FlashCardController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
