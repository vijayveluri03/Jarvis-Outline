using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jarvis
{
    public class Habit : IDirtyable
    {
        public enum Status
        {
            In_Progress = 0,
            Disabled
        }


        public string[] categories;

        // The name or the title for an entry 
        public string title;

        [Obsolete("This will be removed in the next update")]
        [JsonProperty(PropertyName = "startDate")]
        public DateTime _startDateObscelete;

        [JsonProperty(PropertyName = "startDateNew")]
        public Date _startDate;

        // Unique ID
        public int id;

        [Obsolete("This will be removed in the next update")]
        [JsonProperty(PropertyName = "entries")]
        public List<DateTime> _entriesObscelete = new List<DateTime>();


        //@todo - make this private but serializable
        [JsonProperty(PropertyName = "entriesNew")]
        public List<Date> _entries = new List<Date>();

        public Status status = Status.In_Progress;

        [JsonIgnore]
        public bool IsEnabled { get { return status == Status.In_Progress; } }

        [JsonIgnore]
        public bool IsDisabled { get { return status == Status.Disabled; } }

        [JsonIgnore]
        public string StatusStr
        {
            get { return status == Status.In_Progress ? "In-Progress" : "Disabled"; }
        }

        public void Init()
        {
            if (_entriesObscelete != null && _entriesObscelete.Count > 0)
            {
                _entries = new List<Date>();
                foreach (var entry in _entriesObscelete)
                    _entries.Add(new Date(entry));

                _entriesObscelete.Clear();
                IsDirty = true; 
            }

            if(!_startDateObscelete.IsThisMinDate())
            {
                _startDate = (Date)_startDateObscelete;
                _startDateObscelete = DateTime.MinValue;
                IsDirty = true;
            }
        }

        public int GetStreak()
        {
            return -1;
        }

        public int GetAllEntryCount()
        {
            return _entries.Count;
        }

        public void SetStatus(Status status)
        {
            this.status = status;
        }

        public int GetEntryCountForTheDuration(Date start_Inclusive, Date end_Inclusive)
        {
            int count = 0;
            foreach (var entry in _entries)
            {
                if (entry >= start_Inclusive && entry <= end_Inclusive )
                    count++;
            }
            return count;
        }

        // Gets success rate till yesterday. Doesnt include today!
        public int GetSuccessRate ()
        {
            int totalDays = Date.Today - _startDate; // This will not include today
            int totalEntryCount = GetEntryCountForTheDuration(_startDate, Date.Today - 1);

            #if RELEASE_LOG
            foreach (var entry in _entries)
            {
                Utils.Assert(entry >= _startDate && entry <= Date.Today);
            }
            #endif

            return (int)Math.Round( totalEntryCount * 100.0f/ totalDays);
        }

        public bool IsEntryOn(Date date)
        {
            foreach (var entry in _entries)
            {
                if (entry == date)
                    return true;
            }
            return false;
        }

        public bool IsNewEntryValid(Date newEntry)
        {
            return !IsEntryOn(newEntry) && newEntry >= _startDate && newEntry <= Date.Today;
        }

        public void AddNewEntry(Date newEntry)
        {
            Utils.Assert(!IsEntryOn(newEntry));
            Utils.Assert(newEntry >= _startDate);
            Utils.Assert(newEntry <= Date.Today);

            _entries.Add(newEntry);
            _entries.Sort();

            IsDirty = true;
        }

        public Date GetLastUpdatedOn()
        {
            // as the entries are sorted, this should get us the latest update
            if (_entries.Count == 0)
                return _startDate;
            return _entries[_entries.Count - 1];
        }

        public void Reset()
        {
            _entries.Clear();
            _startDate = Date.Today;

            IsDirty = true;
        }
    }

    // List of all the Entries
    public class HabitCollection
    {
        public List<Habit> entries = new List<Habit>();
    }


    public class HabitManager : IDirtyable
    {
        public HabitCollection Data { get; private set; }
        
        public HabitManager()
        {
            if (Utils.CreateFileIfNotExit(JConstants.HABITS_FILENAME, JConstants.HABITS_TEMPLATE_FILENAME))
            {
                ConsoleWriter.Print("Habits data copied from Template. This happens on the first launch.");
            }

            Load(JConstants.HABITS_FILENAME);
        }

        public void AddHabit(Habit ed)
        {
            Data.entries.Add(ed);
            IsDirty = true;
        }

        public void RemoveHabit(Habit ed)
        {
            Data.entries.Remove(ed);
            IsDirty = true;
        }

        private void Load(string fileName)
        {
            using (StreamReader r = new StreamReader(fileName))
            {
                string json = r.ReadToEnd();
                HabitCollection data = JsonConvert.DeserializeObject<HabitCollection>(json);
                Data = data;
            }

            foreach (Habit ed in Data.entries)
            {
                if (ed.id >= availableID)
                    availableID = ed.id + 1;

                ed.Init();
            }
        }

        private bool IsDirtyRecursive()
        {
            foreach (Habit ed in Data.entries)
            {
                if (ed.IsDirty)
                    return true;
            }
            return IsDirty;
        }
        private void UnSetDirtyRecursively()
        {
            foreach (Habit ed in Data.entries)
            {
                ed.IsDirty = false;
            }
            IsDirty = false;
        }

        public void Save()
        {
            if (!IsDirtyRecursive())
                return;

            string serializedData = JsonConvert.SerializeObject(Data, Formatting.Indented);
            File.WriteAllText(JConstants.HABITS_FILENAME, serializedData);
            UnSetDirtyRecursively();

#if RELEASE_LOG
            ConsoleWriter.Print("Habits saved");
#endif
        }

        // Getters

        public bool DoesHabitExist(int id)
        {
            foreach (Habit ed in Data.entries)
            {
                if (ed.id == id)
                    return true;
            }
            return false;
        }

        // Because C# doesnt in const (-|-)
        public Habit GetHabit_ReadOnly(int id)
        {
            foreach (Habit ed in Data.entries)
            {
                if (ed.id == id)
                    return ed;
            }
            return null;
        }

        public Habit GetHabit_Editable(int id)
        {
            IsDirty = true;
            return GetHabit_ReadOnly(id);
        }

        public int GetAvailableID()
        {
            return availableID++;
        }

        // Private 
        private int availableID = 1;
    }

}


