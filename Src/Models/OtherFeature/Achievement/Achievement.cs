﻿using MongoDB.Bson.Serialization.Attributes;

namespace Quizlet_App_Server.Models
{
    [BsonIgnoreExtraElements]
    public class Achievement: Configurable
    {
        public override string SpecialName { get => "Achievement"; set => base.SpecialName = value; }
        [BsonElement("task_list")] public List<Task> TaskList { get; set; } = new List<Task>();
    }
}
