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
            Later,
            Discard
        }

        public string[] categories;

        // The name or the title for an entry 
        public string title;

        // Unique ID
        public int id;

        public string[] subTasks;

        public Status taskStatus = Status.Open;

        // Getters 
        [JsonIgnore] public bool IsOpen { get { return taskStatus == Status.Open; } }
        [JsonIgnore] public bool IsComplete { get { return taskStatus == Status.Complete; } }
        [JsonIgnore] public bool IsDiscarded { get { return taskStatus == Status.Complete; } }
        [JsonIgnore] public bool IsLater { get { return taskStatus == Status.Complete; } }
        [JsonIgnore] public bool IsClosed { get { return IsDiscarded || IsComplete; } }
        [JsonIgnore]
        public string StatusString
        {
            get
            {
                if (IsOpen) return "Open";
                if (IsComplete) return "Completed";
                if (IsDiscarded) return "Discarded";
                if (IsLater) return "Later";
                Utils.Assert(false);
                return "Unknown";
            }
        }

        // Setters ( kinda )
        public void SetAsComplete() { taskStatus = Status.Complete; }
        public void SetAsDiscarded() { taskStatus = Status.Discard; }
        public void SetAsLater() { taskStatus = Status.Later; }
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

        public TaskManager()
        {
            if (Utils.CreateFileIfNotExit(JConstants.OUTLINE_FILENAME, JConstants.OUTLINE_TEMPLATE_FILENAME))
            {
                Console.Out.WriteLine("outline data copied from Template. This happens on the first launch.");
            }

            Load(JConstants.OUTLINE_FILENAME);
        }

        public void AddEntry(Task ed)
        {
            outlineData.entries.Add(ed);
        }
        public bool RemoveEntry(Task ed)
        {
            return outlineData.entries.Remove(ed);
        }
        public bool RemoveEntryIfExists(int id)
        {
            Task ed = GetEntry(id);
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
                if (ed.id >= freeID)
                    freeID = ed.id + 1;
            }
        }
        public void Save()
        {
            string serializedData = JsonConvert.SerializeObject(outlineData, Formatting.Indented);
            File.WriteAllText(JConstants.OUTLINE_FILENAME, serializedData);
        }

        // Getters

        public bool IsEntryAvailableWithID(int id)
        {
            foreach (Task ed in outlineData.entries)
            {
                if (ed.id == id)
                    return true;
            }
            return false;
        }
        public Task GetEntry(int id)
        {
            foreach (Task ed in outlineData.entries)
            {
                if (ed.id == id)
                    return ed;
            }
            return null;
        }

        public int GetNewID()
        {
            return freeID++;
        }

        // Private 
        private int freeID = 1;
    }

}


