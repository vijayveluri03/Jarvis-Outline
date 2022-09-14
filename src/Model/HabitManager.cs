using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jarvis
{
    public enum HabitStatus
    {
        Completed = 0,
        Ignored,
        Missed
    }
    public class HabitEntry : IEquatable<HabitEntry> , IComparable<HabitEntry>
    {
        public HabitEntry() { }
        public HabitEntry(Date date, HabitStatus status) {  this.date = date; this.status = status; }
        public Date date;
        public HabitStatus status;

        public bool IsCompleted {  get {  return this.status == HabitStatus.Completed; } }
        public bool IsIgnored { get { return this.status == HabitStatus.Ignored; } }
        public bool IsMissed {  get { return this.status == HabitStatus.Missed; } }

        public int CompareTo(HabitEntry other)
        {
            if (other == null)
                return 1;
            else
                return this.date.CompareTo(other.date);
        }

        public override bool Equals(object other)
        {
            if (other == null) 
                return false;
            HabitEntry otherHE = other as HabitEntry;
            if (otherHE == null) 
                return false;
            else 
                return Equals(otherHE);
        }
        public bool Equals(HabitEntry other)
        {
            if (other == null) 
                return false;
            return (this.date.Equals(other.date) && this.status.Equals(other.status));
        }
    }
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

        [JsonProperty(PropertyName = "startDateNew")]
        public Date _startDate;

        // Unique ID
        public int id;

        [JsonProperty(PropertyName = "entries")]
        public List<HabitEntry> _entries = new List<HabitEntry>();


        public Status status = Status.In_Progress;

        [JsonProperty(PropertyName = "PreviousTickCount")]
        public int _previousTickCount;

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
        }

        public int GetStreak()
        {
            return -1;
        }

        public int GetAllEntryCount( HabitStatus status, bool includePreviousTickCount = false )
        {
            int count = 0;
            foreach( HabitEntry entry in _entries )
            {
                if (entry.status == status)
                    count++;
            }

            if (includePreviousTickCount)
                return count + _previousTickCount;
            else
                return count;
        }

        public int GetNumberOfDaysFromTheStart()
        {
            return Date.Today - _startDate;
        }

        public void SetStatus(Status status)
        {
            this.status = status;
        }

        public int GetEntryCountForTheDuration( HabitStatus status, Date start_Inclusive, Date end_Inclusive)
        {
            int count = 0;
            foreach (var entry in _entries)
            {
                if ( entry.status == status && entry.date >= start_Inclusive && entry.date <= end_Inclusive )
                    count++;
            }
            return count;
        }

        // Gets success rate till yesterday. Doesnt include today!
        public int GetSuccessRate ( int forTheLastDayCount = -1 )
        {
            Date startDate;
            if (forTheLastDayCount == -1)
                startDate = _startDate;
            else 
                startDate = Date.Today.SubtractDays( forTheLastDayCount );

            if ( startDate < _startDate)
                startDate = _startDate;

            int totalDays = Date.Today - startDate; // This will not include today
            int totalEntryCount = GetEntryCountForTheDuration( HabitStatus.Completed, startDate, Date.Today - 1);
            int totalIgnoreDays = GetEntryCountForTheDuration(HabitStatus.Ignored, startDate, Date.Today - 1);

            totalDays -= totalIgnoreDays; // removing the ignorable days 
#if RELEASE_LOG
            foreach (var entry in _entries)
            {
                Utils.Assert(entry.date >= _startDate && entry.date <= Date.Today);
            }
#endif

            return totalDays > 0 ? (int)Math.Round( totalEntryCount * 100.0f/ totalDays) : 0;
        }

        public int GetSuccessRateForDuration(Date start_Inclusive, Date end_Inclusive)
        {
            int totalDays = Date.Today - start_Inclusive; // This will not include today
            int totalEntryCount = GetEntryCountForTheDuration(HabitStatus.Completed, start_Inclusive, end_Inclusive);
            int totalIgnoreDays = GetEntryCountForTheDuration(HabitStatus.Ignored, start_Inclusive, end_Inclusive);

            totalDays -= totalIgnoreDays; // removing the ignorable days 
#if RELEASE_LOG
            foreach (var entry in _entries)
            {
                Utils.Assert(entry.date >= _startDate && entry.date <= Date.Today);
            }
#endif

            return totalDays > 0 ? (int)Math.Round(totalEntryCount * 100.0f / totalDays) : 0;
        }

        public bool IsTickedOn(Date date)
        {
            foreach (var entry in _entries)
            {
                if (entry.date == date && entry.IsCompleted)
                    return true;
            }
            return false;
        }
        public bool IsIgnoredOn(Date date)
        {
            foreach (var entry in _entries)
            {
                if (entry.date == date && entry.IsIgnored)
                    return true;
            }
            return false;
        }


        public bool IsEntryOn(Date date)
        {
            foreach (var entry in _entries)
            {
                if (entry.date == date)
                    return true;
            }
            return false;
        }


        public bool IsNewEntryValid(Date newEntry, out string error)
        {
            if (IsTickedOn(newEntry) )
            {
                error = "Any entry for the date:" + newEntry.ShortForm() + " already exists!";
                return false;
            }
            if (newEntry < _startDate)
            {
                error = "This habit started on " + newEntry.ShortForm() + ". So an entry before that is invalid.";
                return false;
            }
            if (newEntry > Date.Today)
            {
                error = "Entry represents a future date";
                return false;
            }
            error = string.Empty;
            return  true;
        }

        public void AddNewEntry(Date date, HabitStatus status)
        {
            Utils.Assert(!IsEntryOn(date));
            Utils.Assert(date >= _startDate);
            Utils.Assert(date <= Date.Today);

            _entries.Add( new HabitEntry(date, status));
            _entries.Sort();

            IsDirty = true;
        }

        // Returns date when it was last updated. Returns min value if no update found!
        public Date GetLastUpdatedOn()
        {
            // as the entries are sorted, this should get us the latest update
            if (_entries.Count == 0)
                return Date.MinValue;
            return _entries[_entries.Count - 1].date;
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


