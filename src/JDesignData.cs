using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jarvis {

    // Simple constants which a user or a program does not need to change 

    public static class JConstants {
        public const int ROOT_ENTRY_ID = 0;
        public const string PATH_TO_SOLUTION = "./../../../";

        public const string DESIGNDATA_FILENAME = JConstants.PATH_TO_SOLUTION + ("data/Design.json");
        public const string PLAYERPREFS_FILENAME = JConstants.PATH_TO_SOLUTION + ("data/PlayerPrefs.json");
        public const string OUTLINE_FILENAME = JConstants.PATH_TO_SOLUTION + ("data/Outline.json");
        public const string POMO_FILENAME = JConstants.PATH_TO_SOLUTION + ("data/Pomo.json");

        public const string DESIGNDATA_TEMPLATE_FILENAME = JConstants.PATH_TO_SOLUTION + ("data/Design-Template.json");
        public const string OUTLINE_TEMPLATE_FILENAME = JConstants.PATH_TO_SOLUTION + ("data/Outline-Template.json");
        public const string POMO_TEMPLATE_FILENAME = JConstants.PATH_TO_SOLUTION + ("data/Pomo-Template.json");

        public const string POMO_NOTIFICATION_SOUND = JConstants.PATH_TO_SOLUTION + ("data/Notification.mp3");
        public const int POMO_SMALL_DURATION_IN_MINS = 25;
        public const int POMO_MID_DURATION_IN_MINS = 50;
        public const int POMO_LARGE_DURATION_IN_MINS = 75;
        public const int POMO_HUGE_DURATION_IN_MINS = 150;
    }

    // Load and Save custom data per user.

    [Serializable]
    public class JUserData {

        [JsonProperty] public string userName = string.Empty;
        [JsonProperty] public DateTime lastLoginDate = DateTime.MinValue;
        [JsonProperty] public bool activatePomodoro = true;
        [JsonProperty] public bool activatePomodoroReminder = true;
        [JsonProperty] public List<int> markedTaskIDs = new List<int>();

        public static JUserData Load() {
            if (File.Exists(JConstants.PLAYERPREFS_FILENAME)) {
                using (StreamReader r = new StreamReader(JConstants.PLAYERPREFS_FILENAME)) {
                    string json = r.ReadToEnd();
                    JUserData userData = JsonConvert.DeserializeObject<JUserData>(json);
                    return userData;
                }
            }
            else {
                ConsoleWriter.Print("Player prefs file not found. So defaulting it!");
            }
            return null;
        }

        public void Save() {
            string serializedUserData = JsonConvert.SerializeObject(this);
            File.WriteAllText(JConstants.PLAYERPREFS_FILENAME, serializedUserData);
        }
    }

    // Customizable options like Look and Feel or other attributes which can be changed through settings. 

    public class JDesignData {

        [Serializable]
        public class LookAndFeelProperties {
            [JsonProperty] public int maxColumnsForOptions = 3;
            [JsonProperty] public string defaultColorForText = "White";
            [JsonProperty] public string dueHighlightColorForText = "Red";
        }

        public static JDesignData instance { get; private set; }

        [JsonProperty] public LookAndFeelProperties lookAndFeel;

        // Getters        

        [JsonIgnore] public int ColumnCountForOptions { get { return lookAndFeel.maxColumnsForOptions; } }
        [JsonIgnore] public ConsoleColor defaultColorForText { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.defaultColorForText); } }
        [JsonIgnore] public ConsoleColor dueHighlightColorForText { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.dueHighlightColorForText); } }

        public static JDesignData Load() {
            if (Utils.CreateFileIfNotExit(JConstants.DESIGNDATA_FILENAME, JConstants.DESIGNDATA_TEMPLATE_FILENAME)) {
                ConsoleWriter.Print("Design config copied from Template. This happens on the first launch.");
            }

            using (StreamReader r = new StreamReader(JConstants.DESIGNDATA_FILENAME)) {
                string json = r.ReadToEnd();
                JDesignData data = JsonConvert.DeserializeObject<JDesignData>(json);
                instance = data;
                return data;
            }
        }
    }
}
