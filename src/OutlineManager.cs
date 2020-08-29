using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;



namespace Jarvis {

    // An Entry can be a note or a task 
    public class EntryData {

        // The name or the title for an entry 
        public string title;

        // Unique ID
        public int id;

        // Parent ID 
        public int parentID;

        // Links to other IDs. These can be any other tasks which this task is related to.
        public List<int> links = new List<int>();

        // This is a Bit Mask with 0 : A note, 1: A Task, 2: A Task which is Completed, 4: A Task which is Discarded. 
        public int TaskStatus = 0;  

        // Task only properties
        [JsonProperty]
        private DateTime taskDueDate = DateTime.MinValue;

        [JsonProperty]
        private DateTime taskClosedDate = DateTime.MinValue;

        // Task and Entry properties 
        [JsonProperty] private DateTime createdDate = DateTime.MinValue;

        // Notes Text file
        [JsonProperty] private bool hasNotes = false;

        // Enternal link
        [JsonProperty] private string urlToOpen = "";

        // Getters 

        [JsonIgnore] public bool IsTask { get { return (TaskStatus & 1) > 0; } }
        [JsonIgnore] public bool IsTaskComplete { get { return (TaskStatus & 2) > 0; } }
        [JsonIgnore] public bool IsTaskDiscarded { get { return (TaskStatus & 4) > 0; } }
        [JsonIgnore] public bool IsTaskClosed { get { return IsTaskComplete || IsTaskDiscarded; } }
        [JsonIgnore] public DateTime TaskDueDate { get { return taskDueDate.ZeroTime(); } set { taskDueDate = value.ZeroTime(); } }
        [JsonIgnore] public DateTime TaskClosedDate { get { return taskClosedDate.ZeroTime(); } set { taskClosedDate = value.ZeroTime(); } }
        [JsonIgnore] public DateTime CreatedDate { get { return createdDate.ZeroTime(); } set { createdDate = value.ZeroTime(); } }
        [JsonIgnore] public string URLToOpen { get { return urlToOpen; } set { urlToOpen = value; } }
        [JsonIgnore] public bool DoesUrlExist { get { return !string.IsNullOrEmpty(urlToOpen); } }
        [JsonIgnore] public int DaysRemainingFromDueDate {  get { return (TaskDueDate - Utils.Now).Days; } }

        // Setters ( kinda )

        public void SetAsTask(DateTime time) { TaskStatus |= 1; taskDueDate = time.ZeroTime(); }
        public void SetAsComplete(DateTime completedDate) { Utils.Assert(IsTask); TaskStatus |= 2; taskClosedDate = completedDate.ZeroTime(); }
        public void SetAsDiscarded(DateTime discardedDate) { Utils.Assert(IsTask); TaskStatus |= 4; taskClosedDate = discardedDate.ZeroTime(); }
        public void ConvertToEntry() { TaskStatus = 0; taskDueDate = DateTime.MinValue; }
        public void AddLink(int id) { if (!links.Contains(id)) links.Add(id); }
        public void UnLink(int id) { if (links.Contains(id)) links.Remove(id); }
    }

    // List of all the Entries

    public class OutlineData {
        public List<EntryData> entries = new List<EntryData>();
    }

    // A Manager for all the entries

    public class OutlineManager {

        public OutlineData outlineData { get; private set; }

        public OutlineManager() {
            if (Utils.CreateFileIfNotExit(JConstants.OUTLINE_FILENAME, JConstants.OUTLINE_TEMPLATE_FILENAME)) {
                ConsoleWriter.Print("outline data copied from Template. This happens on the first launch.");
            }

            Load(JConstants.OUTLINE_FILENAME);
        }

        // Add or Remove entries from the list

        public void AddEntry(EntryData ed) {
            outlineData.entries.Add(ed);
        }
        public void RemoveEntry(EntryData ed) {
            outlineData.entries.Remove(ed);
        }


        // Load or Save the entries

        private void Load(string fileName) {

            using (StreamReader r = new StreamReader(fileName)) {
                string json = r.ReadToEnd();
                OutlineData data = JsonConvert.DeserializeObject<OutlineData>(json);
                outlineData = data;
            }

            foreach (EntryData ed in outlineData.entries) {
                if (ed.id >= freeID)
                    freeID = ed.id + 1;
            }
        }
        public void Save() {
            string serializedData = JsonConvert.SerializeObject(outlineData, Formatting.Indented);
            File.WriteAllText(JConstants.OUTLINE_FILENAME, serializedData);
        }

        // Getters

        public bool IsEntryAvailableWithID(int id) {
            foreach (EntryData ed in outlineData.entries) {
                if (ed.id == id)
                    return true;
            }
            return false;
        }
        public EntryData GetEntry(int id) {
            foreach (EntryData ed in outlineData.entries) {
                if (ed.id == id)
                    return ed;
            }
            return null;
        }

        public int GetNewID() {
            return freeID++;
        }

        // Private 
        private int freeID = 1;
    }
}
