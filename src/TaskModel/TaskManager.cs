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

        public List< Pair<int, string> > subTasks = new List<Pair<int, string>>();

        public Status taskStatus = Status.Open;

        public Type type = Type.Task;

        [JsonIgnore] public bool isDirty = false;

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

        public int GetSubTaskCount()
        {
            return subTasks != null ? subTasks.Count: 0;
        }

        private int GetAvailableID()
        {
            int maxID = 0;
            foreach (var subtaskPair in subTasks)
            {
                if (subtaskPair.First > maxID)
                    maxID = subtaskPair.First;
            }
            return maxID + 1;
        }

        public Pair<int, string> GetSubtask (int id)
        {
            foreach (var subtaskPair in subTasks)
            {
                if (subtaskPair.First == id)
                    return subtaskPair;
            }
            return null;
        }

        public void AddSubTask(string subTask)
        {
            subTasks.Add(new Pair<int, string>(GetAvailableID(), subTask));
        }

        public void RemoveSubTask(int id)
        {
            var subTaskPair = GetSubtask(id);
            Utils.Assert(subTaskPair != null);
            subTasks.Remove(subTaskPair);
        }
    }

    // List of all the Entries
    public class TaskCollection
    {
        public List<Task> entries = new List<Task>();
    }


    public class TaskManager
    {
        public TaskCollection Data { get; private set; }
        private bool dirty = false;

        public TaskManager()
        {
            if (Utils.CreateFileIfNotExit(JConstants.TASKS_FILENAME, JConstants.TASKS_TEMPLATE_FILENAME))
            {
                ConsoleWriter.Print("Task data copied from Template. This happens on the first launch.");
            }

            Load(JConstants.TASKS_FILENAME);
        }

        public void AddTask(Task ed)
        {
            Data.entries.Add(ed);
            dirty = true;
        }
        public bool RemoveTask(Task ed)
        {
            dirty = true;
            return Data.entries.Remove(ed);
        }
        public bool RemoveTaskIfExists(int id)
        {
            dirty = true;

            Task ed = GetTask_ReadOnly(id);
            if (ed != null)
                return Data.entries.Remove(ed);
            return false;
        }


        private void Load(string fileName)
        {
            using (StreamReader r = new StreamReader(fileName))
            {
                string json = r.ReadToEnd();
                TaskCollection data = JsonConvert.DeserializeObject<TaskCollection>(json);
                Data = data;
            }

            foreach (Task ed in Data.entries)
            {
                if (ed.id >= availableID)
                    availableID = ed.id + 1;
            }
        }
        public void Save()
        {
            if (!dirty)
                return;

            string serializedData = JsonConvert.SerializeObject(Data, Formatting.Indented);
            File.WriteAllText(JConstants.TASKS_FILENAME, serializedData);
            dirty = false;

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


