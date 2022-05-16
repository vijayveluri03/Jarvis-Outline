using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jarvis
{
    public class Task
    {
        public enum Status
        {
            Open,
            Complete,
            Archieve,
            Discard
        }

        public enum Type
        {
            Task, 
            Story
        }

        public string[] categories;

        // The name or the title for an entry 
        public string title;

        // Unique ID
        public int id;

        public string[] subTasks;

        public Status taskStatus = Status.Open;

        public Type type = Type.Task;

        // Getters 
        [JsonIgnore] public bool IsOpen { get { return taskStatus == Status.Open; } }
        [JsonIgnore] public bool IsComplete { get { return taskStatus == Status.Complete; } }
        [JsonIgnore] public bool IsDiscarded { get { return taskStatus == Status.Discard; } }
        [JsonIgnore] public bool IsArchieved { get { return taskStatus == Status.Archieve; } }
        [JsonIgnore] public bool IsClosed { get { return IsDiscarded || IsComplete; } }
        [JsonIgnore] public bool IsTask { get { return type == Type.Task; } }
        [JsonIgnore] public bool IsStory { get { return type == Type.Story; } }

        [JsonIgnore]
        public string StatusString
        {
            get
            {
                if (IsOpen) return "Open";
                if (IsComplete) return "Completed";
                if (IsDiscarded) return "Discarded";
                if (IsArchieved) return "Archieved";
                Utils.Assert(false);
                return "Unknown";
            }
        }
        [JsonIgnore]
        public string TypeString
        {
            get
            {
                if (IsTask) return "Task";
                if (IsStory) return "Story";
                Utils.Assert(false);
                return "Unknown";
            }
        }

        // Setters ( kinda )
        public void SetAsComplete() { taskStatus = Status.Complete; }
        public void SetAsDiscarded() { taskStatus = Status.Discard; }
        public void SetAsArchive() { taskStatus = Status.Archieve; }
        public void SetStatus(Status status) { taskStatus = status; }
        public void AddSubTask( string subTask )
        {
            // TODO - optimize this 
            List<string> tasks = subTasks != null ? new List<string> (subTasks) : new List<string>();
            tasks.Add(subTask);
            subTasks = tasks.ToArray();
        }
    }

    // List of all the Entries
    public class TaskCollection
    {
        public List<Task> entries = new List<Task>();
    }


    public class TaskManager
    {
        public TaskCollection outlineData { get; private set; }
        private bool dirty = false;

        public TaskManager()
        {
            if (Utils.CreateFileIfNotExit(JConstants.OUTLINE_FILENAME, JConstants.OUTLINE_TEMPLATE_FILENAME))
            {
                ConsoleWriter.Print("outline data copied from Template. This happens on the first launch.");
            }

            Load(JConstants.OUTLINE_FILENAME);
        }

        public void AddTask(Task ed)
        {
            outlineData.entries.Add(ed);
            dirty = true;
        }
        public bool RemoveTask(Task ed)
        {
            dirty = true;
            return outlineData.entries.Remove(ed);
        }
        public bool RemoveTaskIfExists(int id)
        {
            dirty = true;

            Task ed = GetTask_ReadOnly(id);
            if (ed != null)
                return outlineData.entries.Remove(ed);
            return false;
        }


        private void Load(string fileName)
        {
            using (StreamReader r = new StreamReader(fileName))
            {
                string json = r.ReadToEnd();
                TaskCollection data = JsonConvert.DeserializeObject<TaskCollection>(json);
                outlineData = data;
            }

            foreach (Task ed in outlineData.entries)
            {
                if (ed.id >= availableID)
                    availableID = ed.id + 1;
            }
        }
        public void Save()
        {
            if(!dirty)
                return;

            string serializedData = JsonConvert.SerializeObject(outlineData, Formatting.Indented);
            File.WriteAllText(JConstants.OUTLINE_FILENAME, serializedData);
            dirty = false;

            #if RELEASE_LOG
            ConsoleWriter.Print("Tasks saved");
            #endif
        }

        // Getters

        public bool DoesTaskExist(int id)
        {
            foreach (Task ed in outlineData.entries)
            {
                if (ed.id == id)
                    return true;
            }
            return false;
        }
        
        // Because C# doesnt in const (-|-)
        public Task GetTask_ReadOnly(int id)
        {
            foreach (Task ed in outlineData.entries)
            {
                if (ed.id == id)
                    return ed;
            }
            return null;
        }

        public Task GetTask_Editable(int id)
        {
            dirty = true;
            return GetTask_ReadOnly(id);
        }

        public int GetAvailableID()
        {
            return availableID++;
        }

        // Private 
        private int availableID = 1;
    }

}


