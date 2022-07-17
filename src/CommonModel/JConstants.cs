using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jarvis
{

    // Simple constants which a user or a program does not need to change 

    public static class JConstants
    {
        public const int ROOT_ENTRY_ID = 0;

        public static string WORKING_DIRECTORY = "";
        public static string PATH_TO_DATA { get { return WORKING_DIRECTORY + "data/"; } }
        public static string PATH_TO_NOTES { get { return WORKING_DIRECTORY + "data/Notes/"; } }

        public static string DESIGNDATA_FILENAME { get { return JConstants.PATH_TO_DATA + ("Design.json"); } }
        public static string USERDATA_FILENAME { get { return JConstants.PATH_TO_DATA + ("UserData.json"); } }
        public static string TASKS_FILENAME { get { return JConstants.PATH_TO_DATA + ("Tasks.json"); } }
        public static string JOURNAL_FILENAME { get { return JConstants.PATH_TO_DATA + ("Notebook.json"); } }
        public static string POMODORO_FILENAME { get { return JConstants.PATH_TO_DATA + ("Pomodoro.json"); } }
        public static string HABITS_FILENAME { get { return JConstants.PATH_TO_DATA + ("Habits.json"); } }
        public static string TASK_LOG_FILENAME { get { return JConstants.PATH_TO_DATA + ("TaskLog.json"); } }

        public static string PATH_TO_TASKS_NOTE { get { return JConstants.PATH_TO_DATA + ("Tasks/"); } }
        public static string PATH_TO_HABITS_NOTE { get { return JConstants.PATH_TO_DATA + ("Habits/"); } }
        public static string PATH_TO_TASK_LOG_NOTES { get { return JConstants.PATH_TO_DATA + ("TaskLog/"); } }
        public static string PATH_TO_JOURNAL_NOTE { get { return JConstants.PATH_TO_DATA + ("Notebook/"); } }

        public static string DESIGNDATA_TEMPLATE_FILENAME { get { return JConstants.PATH_TO_DATA + ("Design-Template.json"); } }
        public static string TASKS_TEMPLATE_FILENAME { get { return JConstants.PATH_TO_DATA + ("Tasks-Template.json"); } }
        public static string HABITS_TEMPLATE_FILENAME { get { return JConstants.PATH_TO_DATA + ("Habits-Template.json"); } }
        public static string TASK_LOG_TEMPLATE_FILENAME { get { return JConstants.PATH_TO_DATA + ("TaskLog-Template.json"); } }
        public static string JOURNAL_TEMPLATE_FILENAME { get { return JConstants.PATH_TO_DATA + ("Notebook-Template.json"); } }
        public static string POMODORO_TEMPLATE_FILENAME { get { return JConstants.PATH_TO_DATA + ("Pomodoro-Template.json"); } }

        public static int POMODORO_WORK_TIME { get { return 25; } }
        public static int POMODORO_REST_TIME { get { return 5; } }
        public static int POMODORO_LONG_REST_TIME { get { return 15; } }
    }

}
