using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jarvis
{
    public class Task
    {
        public enum Type
        {
            Task,
            Story,
            Collection
        }

        public string[] categories;

        // The name or the title for an entry 
        public string title;

        // Unique ID
        public int id;

        public string status = string.Empty;

        public Type type = Type.Task;

        [JsonIgnore] public bool isDirty = false;

        // Getters 
        [JsonIgnore] public bool IsTask { get { return type == Type.Task; } }
        [JsonIgnore] public bool IsStory { get { return type == Type.Story; } }
        [JsonIgnore] public bool IsCollection { get { return type == Type.Collection; } }

        [JsonIgnore]
        public string StatusString
        {
            get
            {
                if (status.IsEmpty()) return "Unknown";
                return status;
            }
        }
        [JsonIgnore]
        public string TypeString
        {
            get
            {
                if (IsTask) return "Task";
                if (IsStory) return "Story";
                if (IsCollection) return "Collection";
                Utils.Assert(false);
                return "Unknown";
            }
        }

        // Setters ( kinda )
        public void SetStatus( string status) { this.status = status; }
    }

    // List of all the Entries
    public class TaskCollection
    {
        public List<Task> entries = new List<Task>();
    }


    public class TaskManager : IDirtyable
    {
        public TaskCollection Data { get; private set; }

        public TaskManager( string defaultStatus, System.Func<string, bool> isStatusValid )
        {
            if (Utils.CreateFileIfNotExit(JConstants.TASKS_FILENAME, JConstants.TASKS_TEMPLATE_FILENAME))
            {
                ConsoleWriter.Print("Task data copied from Template. This happens on the first launch.");
            }
            LoadData(JConstants.TASKS_FILENAME);
            PostLoad(defaultStatus, isStatusValid);
        }

        public void AddTask(Task ed)
        {
            Data.entries.Add(ed);
            IsDirty = true;
        }
        public bool RemoveTask(Task ed)
        {
            IsDirty = true;
            return Data.entries.Remove(ed);
        }
        public bool RemoveTaskIfExists(int id)
        {
            IsDirty = true;

            Task ed = GetTask_ReadOnly(id);
            if (ed != null)
                return Data.entries.Remove(ed);
            return false;
        }


        private void LoadData(string fileName)
        {
            using (StreamReader r = new StreamReader(fileName))
            {
                string json = r.ReadToEnd();
                TaskCollection data = JsonConvert.DeserializeObject<TaskCollection>(json);
                Data = data;
            }
        }

        private void PostLoad(string defaultStatus, System.Func<string, bool> isStatusValid)
        {
            // This is for data migration for older versions or from previous statuses to the new ones.
            foreach (Task ed in Data.entries)
            {
                if (ed.status.IsEmpty() || !isStatusValid( ed.status))
                {
                    ed.SetStatus(defaultStatus);
                    IsDirty = true;
                }
            }
        }

        public void Save()
        {
            if (!IsDirty)
                return;

            string serializedData = JsonConvert.SerializeObject(Data, Formatting.Indented);
            File.WriteAllText(JConstants.TASKS_FILENAME, serializedData);
            IsDirty = false;

#if RELEASE_LOG
            ConsoleWriter.Print("Tasks saved");
#endif
        }

        // Getters

        public bool DoesTaskExist(int id)
        {
            foreach (Task ed in Data.entries)
            {
                if (ed.id == id)
                    return true;
            }
            return false;
        }

        // Because C# doesnt in const (-|-)
        public Task GetTask_ReadOnly(int id)
        {
            foreach (Task ed in Data.entries)
            {
                if (ed.id == id)
                    return ed;
            }
            return null;
        }

        public Task GetTask_Editable(int id)
        {
            IsDirty = true;
            return GetTask_ReadOnly(id);
        }

        public int GenerateNextAvailableID()
        {
            for ( int id = 1; id < int.MaxValue; id ++ )
            {
                if (!DoesTaskExist(id))
                    return id;
            }
            return -1;
        }
    }

}


