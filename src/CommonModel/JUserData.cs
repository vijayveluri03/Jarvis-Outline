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
        public class PomodoroProgress
        {
            [JsonProperty] public DateTime startTime = DateTime.MinValue;
            [JsonProperty] public int pomoCount = 0;
            [JsonProperty] public string category = "";
        }


        [JsonProperty] public string userName = string.Empty;
        [JsonProperty] public DateTime lastLoginDate = DateTime.MinValue;
        [JsonProperty] public TaskProgress taskProgress = new TaskProgress();
        [JsonProperty] public PomodoroProgress pomodoroProgress = new PomodoroProgress();
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
        public void StartPomodoro(DateTime time, string category, int count)
        {
            lock (lockObj)
            {
                pomodoroProgress.startTime = time;
                pomodoroProgress.pomoCount = count;
                pomodoroProgress.category = category;
                IsDirty = true;
            }
        }
        public void StopPomodoro()
        {
            lock (lockObj)
            {
                pomodoroProgress.startTime = DateTime.MinValue;
                pomodoroProgress.pomoCount = 0;
                pomodoroProgress.category = "";
                IsDirty = true;
            }
        }
        public bool IsPomodoroInProgress()
        {
            return !pomodoroProgress.startTime.IsThisMinDate();
        }
        public bool IsPomodoroCountUpTimer()
        {
            return IsPomodoroInProgress() && pomodoroProgress.pomoCount == -1;
        }
        public DateTime GetPomodoroStartTime()
        {
            return pomodoroProgress.startTime;
        }
        public PomodoroProgress GetPomodoroData()
        {
            return pomodoroProgress;
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
