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
        public static string PLAYERPREFS_FILENAME { get { return JConstants.PATH_TO_DATA + ("PlayerPrefs.json"); } }
        public static string OUTLINE_FILENAME { get { return JConstants.PATH_TO_DATA + ("Outline.json"); } }
        public static string TASK_LOG_FILENAME { get { return JConstants.PATH_TO_DATA + ("TaskLog.json"); } }

        public static string DESIGNDATA_TEMPLATE_FILENAME { get { return JConstants.PATH_TO_DATA + ("Design-Template.json"); } }
        public static string OUTLINE_TEMPLATE_FILENAME { get { return JConstants.PATH_TO_DATA + ("Outline-Template.json"); } }
        public static string TASK_LOG_TEMPLATE_FILENAME { get { return JConstants.PATH_TO_DATA + ("TaskLog-Template.json"); } }
    }

    // Load and Save custom data per user.

    [Serializable]
    public class JUserData
    {
        [Serializable]
        public class TaskProgress
        {
            [JsonProperty] public int taskIDInProgress = -1;
            [JsonProperty] public DateTime startTime = DateTime.MinValue;
        }


        [JsonProperty] public string userName = string.Empty;
        [JsonProperty] public DateTime lastLoginDate = DateTime.MinValue;
        [JsonProperty] public TaskProgress taskProgress = new TaskProgress();

        [JsonIgnore] private bool dirty = false;

        public static JUserData Load()
        {
            if (File.Exists(JConstants.PLAYERPREFS_FILENAME))
            {
                using (StreamReader r = new StreamReader(JConstants.PLAYERPREFS_FILENAME))
                {
                    string json = r.ReadToEnd();
                    JUserData userData = JsonConvert.DeserializeObject<JUserData>(json);
                    return userData;
                }
            }
            else
            {
                Console.WriteLine("Player prefs file not found. So defaulting it!");
                return new JUserData();
            }
        }

        public void StartTask(int id, DateTime time)
        {
            taskProgress.taskIDInProgress = id;
            taskProgress.startTime = time;

            dirty = true;
        }
        public void StopTask()
        {
            taskProgress.taskIDInProgress = -1;

            dirty = true;
        }
        public bool IsTaskInProgress()
        {
            return taskProgress.taskIDInProgress != -1;
        }


        public void Save()
        {
            if (!dirty)
                return;

            string serializedUserData = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(JConstants.PLAYERPREFS_FILENAME, serializedUserData);
            dirty = false;

            #if RELEASE_LOG
            Console.WriteLine("Userdata saved");
            #endif
        }
    }

    // Customizable options like Look and Feel or other attributes which can be changed through settings. 

    public class JDesignData
    {

        [Serializable]
        public class LookAndFeelProperties
        {
            [JsonProperty] public int maxColumnsForOptions = 3;
            [JsonProperty] public string defaultColorForText = "White";
            [JsonProperty] public string dueHighlightColorForText = "Red";
        }
        [Serializable]
        public class Categories
        {
            [JsonProperty] public string[] listOfCategories;
        }

        public static JDesignData instance { get; private set; }

        [JsonProperty] public LookAndFeelProperties lookAndFeel;
        [JsonProperty] public Categories categories;

        // Getters        

        [JsonIgnore] public int ColumnCountForOptions { get { return lookAndFeel.maxColumnsForOptions; } }
        [JsonIgnore] public ConsoleColor defaultColorForText { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.defaultColorForText); } }
        [JsonIgnore] public ConsoleColor dueHighlightColorForText { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.dueHighlightColorForText); } }

        public static JDesignData Load()
        {
            if (Utils.CreateFileIfNotExit(JConstants.DESIGNDATA_FILENAME, JConstants.DESIGNDATA_TEMPLATE_FILENAME))
            {
                ConsoleWriter.Print("Design config copied from Template. This happens on the first launch.");
            }

            using (StreamReader r = new StreamReader(JConstants.DESIGNDATA_FILENAME))
            {
                string json = r.ReadToEnd();
                JDesignData data = JsonConvert.DeserializeObject<JDesignData>(json);
                instance = data;
                return data;
            }
        }

        public bool DoesCategoryExist(string category)
        {
            foreach (var cat in categories.listOfCategories)
            {
                if (cat == category)
                    return true;
            }
            return false;
        }
        public bool DoesCategoryExist(string[] categories)
        {
            Utils.Assert(categories.Length > 0);
            foreach (var cat in categories)
            {
                if (!DoesCategoryExist(cat))
                    return false;
            }
            return true;
        }
    }
}
