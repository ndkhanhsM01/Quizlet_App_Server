﻿namespace Quizlet_App_Server.Utility
{
    public class VariableConfig
    {
        public const int BaseScore = 5;
        public const string DatabaseName = "QuizletApp";
        public const string ConnectionString = "mongodb+srv://Admin_ndkhanhs:5xRWMqf08L88sEZF@ndkhanhsfirst.xdi6ojo.mongodb.net/?retryWrites=true&w=majority";
        public const string Collection_Users = "users";
        public const string Collection_UserSequence = "UserSequence";
        public const string Collection_Configure = "Configure";
        public const string Collection_Admin = "Admin";
        public const string Collection_StudySetPublic = "StudySetPublic";

        // Tetris
        public const string Tetris_DatabaseName = "TetrisGame";
        public const string Collection_TetrisScore = "UserScore";
    }
}
