using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jarvis
{

    // Load and Save custom data per user.

    [Serializable]
    public class JUserData : IDirtyable
    {
        [Serializable]
        public class TaskProgress
        {
            [JsonProperty] public int taskIDInProgress = -1;
            [JsonProperty] public DateTime startTime = DateTime.MinValue;
        }

        [Serializable]
        public class PomodoroParams
        {
            [JsonProperty] public DateTime startTime = DateTime.MinValue;
            [JsonProperty] public int sessionType = 0; // this is a custom type which only pomodoro manager knows about
            [JsonProperty] public string category = "";
        }


        [JsonProperty] public string userName = string.Empty;
        [JsonProperty] public DateTime lastLoginDate = DateTime.MinValue;
        [JsonProperty] public TaskProgress taskProgress = new TaskProgress();
        [JsonProperty] public PomodoroParams pomodoroParams = new PomodoroParams();
        [JsonIgnore] public string lastCommandUsed = "";

        public static JUserData Load()
        {
            if (File.Exists(JConstants.USERDATA_FILENAME))
            {
                using (StreamReader r = new StreamReader(JConstants.USERDATA_FILENAME))
                {
                    string json = r.ReadToEnd();
                    JUserData userData = JsonConvert.DeserializeObject<JUserData>(json);
                    return userData;
                }
            }
            else
            {
                ConsoleWriter.Print("Player prefs file not found. So defaulting it!");
                var userData = new JUserData();
                userData.IsDirty = true;
                return userData;
            }
        }

        public void StartTask(int id, DateTime time)
        {
            taskProgress.taskIDInProgress = id;
            taskProgress.startTime = time;

            IsDirty = true;
        }
        public void StopTask()
        {
            taskProgress.taskIDInProgress = -1;

            IsDirty = true;
        }
        public bool IsTaskInProgress()
        {
            return taskProgress.taskIDInProgress != -1;
        }

        #region POMODORO
        public void StorePomodoroStatus(DateTime time, string category, int type)
        {
            lock (lockObj)
            {
                pomodoroParams.startTime = time;
                pomodoroParams.sessionType = type;
                pomodoroParams.category = category;
                IsDirty = true;
            }
        }
        public void ResetPomodoroStatus()
        {
            lock (lockObj)
            {
                pomodoroParams.startTime = DateTime.MinValue;
                pomodoroParams.sessionType = -1;
                pomodoroParams.category = "";
                IsDirty = true;
            }
        }
        public bool IsPomodoroSessionInProgress()
        {
            return !pomodoroParams.startTime.IsThisMinDate();
        }
        
        public DateTime GetPomodoroStartTime()
        {
            return pomodoroParams.startTime;
        }
        public PomodoroParams GetPomodoroData()
        {
            return pomodoroParams;
        }

        private static readonly Object lockObj = new Object();
        #endregion

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
            if (!IsDirty)
                return;

            string serializedUserData = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(JConstants.USERDATA_FILENAME, serializedUserData);
            IsDirty = false;

#if RELEASE_LOG
            ConsoleWriter.Print("Userdata saved");
#endif
        }
    }

}
