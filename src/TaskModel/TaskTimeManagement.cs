using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jarvis
{
    public class LogEntry
    {
        public int id;
        public DateTime date;
        public int timeTakenInMinutes;
        public string comment;
    }

    public class LogCollection
    {
        public List<LogEntry> entries = new List<LogEntry>();
    }

    public class TaskTimeManagement
    {
        public LogCollection logs { get; private set; }
        private bool dirty = false;

        public TaskTimeManagement()
        {
            if (Utils.CreateFileIfNotExit(JConstants.TASK_LOG_FILENAME, JConstants.TASK_LOG_TEMPLATE_FILENAME))
            {
                ConsoleWriter.Print("Task Log data copied from Template. This happens on the first launch.");
            }

            Load(JConstants.TASK_LOG_FILENAME);
        }

        // Add or Remove entries from the list

        public void AddEntry(LogEntry ed)
        {
            logs.entries.Add(ed);

            dirty = true;
        }

        public void RemoveAllEntries(int taskID)
        {
            logs.entries.RemoveAll(e => { return e.id == taskID; } );

            dirty = true;
        }

        public int GetTotalTimeSpentToday(int id)
        {
            int totalTime = 0;
            foreach (var log in logs.entries)
            {
                if ( log.id == id && log.date.IsToday() )
                    totalTime += log.timeTakenInMinutes;
            }
            return totalTime;
        }

        public int GetTotalTimeSpentInMins(int id)
        {
            int totalTime = 0;
            foreach (var log in logs.entries)
            {
                if ( log.id == id )
                    totalTime += log.timeTakenInMinutes;
            }
            return totalTime;
        }


        // Load or Save the entries

        private void Load(string fileName)
        {
            using (StreamReader r = new StreamReader(fileName))
            {
                string json = r.ReadToEnd();
                LogCollection data = JsonConvert.DeserializeObject<LogCollection>(json);
                logs = data;
            }
        }
        public void Save()
        {
            if(!dirty)
                return;

            string serializedData = JsonConvert.SerializeObject(logs, Formatting.Indented);
            File.WriteAllText(JConstants.TASK_LOG_FILENAME, serializedData);
            dirty = false;

            #if RELEASE_LOG
            ConsoleWriter.Print("Logs saved");
            #endif
        }
    }
}
