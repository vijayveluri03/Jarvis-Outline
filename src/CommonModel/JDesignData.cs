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
        public static string TASKS_FILENAME { get { return JConstants.PATH_TO_DATA + ("Tasks.json"); } }
        public static string JOURNAL_FILENAME { get { return JConstants.PATH_TO_DATA + ("Journal.json"); } }
        public static string HABITS_FILENAME { get { return JConstants.PATH_TO_DATA + ("Habits.json"); } }
        public static string TASK_LOG_FILENAME { get { return JConstants.PATH_TO_DATA + ("TaskLog.json"); } }

        public static string PATH_TO_TASKS_NOTE { get { return JConstants.PATH_TO_DATA + ("Tasks/"); } }
        public static string PATH_TO_HABITS_NOTE { get { return JConstants.PATH_TO_DATA + ("Habits/"); } }
        public static string PATH_TO_TASK_LOG_NOTES { get { return JConstants.PATH_TO_DATA + ("TaskLog/"); } }
        public static string PATH_TO_JOURNAL_NOTE { get { return JConstants.PATH_TO_DATA + ("Journal/"); } }

        public static string DESIGNDATA_TEMPLATE_FILENAME { get { return JConstants.PATH_TO_DATA + ("Design-Template.json"); } }
        public static string TASKS_TEMPLATE_FILENAME { get { return JConstants.PATH_TO_DATA + ("Tasks-Template.json"); } }
        public static string HABITS_TEMPLATE_FILENAME { get { return JConstants.PATH_TO_DATA + ("Habits-Template.json"); } }
        public static string TASK_LOG_TEMPLATE_FILENAME { get { return JConstants.PATH_TO_DATA + ("TaskLog-Template.json"); } }
        public static string JOURNAL_TEMPLATE_FILENAME { get { return JConstants.PATH_TO_DATA + ("Journal-Template.json"); } }
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
        [JsonIgnore] public string lastCommandUsed = "";

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
                ConsoleWriter.Print("Player prefs file not found. So defaulting it!");
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
        public string GetLastCommand()
        {
            return lastCommandUsed;
        }
        public void SetCommandUsed(string command)
        {
            if (command != lastCommandUsed)
            {
                lastCommandUsed = command;
            }
        }


        public void Save()
        {
            if (!dirty)
                return;

            string serializedUserData = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(JConstants.PLAYERPREFS_FILENAME, serializedUserData);
            dirty = false;

#if RELEASE_LOG
            ConsoleWriter.Print("Userdata saved");
#endif
        }
    }

    // Customizable options like Look and Feel or other attributes which can be changed through settings. 

    public class JDesignData
    {

        [Serializable]
        public class LookAndFeelProperties
        {
            [JsonProperty] public string defaultColorForText = "White";
            [JsonProperty] public string highlightColorForText = "White";
            [JsonProperty] public string highlightColorForText_2 = "White";
            [JsonProperty] public string highlightColorForText_3 = "White";
            [JsonProperty] public string highlightColorForText_4 = "White";
            [JsonProperty] public string highlightColorForText_5 = "White";
            [JsonProperty] public string highlightColorForText_disabled = "White";
        }
        [Serializable]
        public class Categories
        {
            [JsonProperty] public string[] listOfCategories;
        }
        [Serializable]
        public class Tasks
        {
            [JsonProperty] public string[] statusList;
            [JsonProperty] public string defaultStatus;
            [JsonProperty] public string completeStatus;
        }

        public static JDesignData instance { get; private set; }

        [JsonProperty] public LookAndFeelProperties lookAndFeel;
        [JsonProperty] public Categories categories;
        [JsonProperty] public Tasks tasks;
        [JsonProperty] public string defaultExternalEditor = "code";

        // Getters        
        [JsonIgnore] public ConsoleColor DefaultColorForText { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.defaultColorForText); } } // used for all default text
        [JsonIgnore] public ConsoleColor HighlightColorForText { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.highlightColorForText); } } //used for headings 
        [JsonIgnore] public ConsoleColor HighlightColorForText_2 { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.highlightColorForText_2); } } // used to highlight stories
        [JsonIgnore] public ConsoleColor HighlightColorForText_3 { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.highlightColorForText_3); } } // used to highlight collections 
        [JsonIgnore] public ConsoleColor HighlightColorForText_4 { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.highlightColorForText_4); } } // unused
        [JsonIgnore] public ConsoleColor HighlightColorForText_5 { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.highlightColorForText_5); } } // unused
        [JsonIgnore] public ConsoleColor HighlightColorForText_Disabled { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.highlightColorForText_disabled); } }    // used to represent disabled or inactive stuff
        [JsonIgnore] private HashSet<string> cachedTaskStatuses = new HashSet<string>();

        [JsonIgnore] public string DefaultStatus { get { return tasks.defaultStatus; } }
        [JsonIgnore] public string CompletedStatus { get { return tasks.completeStatus; } }

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
                instance.PostLoad();
                return data;
            }
        }

        private void PostLoad()
        {
            foreach (var status in tasks.statusList)
            {
                if (cachedTaskStatuses.Contains(status))
                {
                    ConsoleWriter.Print("Error : Statuses are repeated in Data/Design.Json");
                    //@todo raise exception ?
                    continue;
                }
                cachedTaskStatuses.Add(status);
            }
            if (!cachedTaskStatuses.Contains(tasks.defaultStatus))
            {
                ConsoleWriter.Print("Error : Default status is invalid in Data/Design.Json. It should be one of the pre defined statuses.");
                //@todo raise exception ?
            }
            if (!cachedTaskStatuses.Contains(tasks.completeStatus))
            {
                ConsoleWriter.Print("Error : Complete status is invalid in Data/Design.Json. It should be one of the pre defined statuses.");
                //@todo raise exception ?
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

        public bool DoesStatusExist(string status)
        {
            return cachedTaskStatuses.Contains(status);
        }

        public bool DoesStatusExistFuzzySearch(string status)
        {
                status = status.ToLower();
                foreach (var st in cachedTaskStatuses)
                {
                    if (st.ToLower().Contains(status))
                    {
                        return true;
                    }
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

        public string GetStatusesAsCommaSeperatedString()
        {
            return Utils.Conversions.ArrayToString(tasks.statusList, true);
        }
    }
}
