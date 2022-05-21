using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jarvis
{
    public class Habit
    {
        public string[] categories;

        // The name or the title for an entry 
        public string title;

        public DateTime startDate;

        // Unique ID
        public int id;

        public List<string> notes = new List<string>();

        public List<DateTime> entries = new List<DateTime>();

        public int previousStreak = 0;

        public int GetStreak()
            {
            return entries.Count + previousStreak;
            }

        public int GetEntryCount()
        {
            return entries.Count;
        }
        public int GetEntryCount(int dayCount )
        {
            int count = 0;
            Utils.Assert( dayCount >= 0 );
            foreach( var entry in entries)
            {
                if ( entry.ZeroTime() >= DateTime.Now.AddDays( -1 * dayCount ).ZeroTime() )
                    count++;
            }
            return count;
        }

        public bool IsEntryOn(DateTime date)
        {
            date = date.ZeroTime();
            foreach( var entry in entries)
            {
                if ( entry.ZeroTime() == date)
                    return true;
            }
            return false;
        }

        public void AddNewEntry( DateTime newEntry)
        {
            Utils.Assert( !IsEntryOn(newEntry));
            entries.Add(newEntry);
        }

        public DateTime GetLastUpdatedOn()
        {
            if( entries.Count == 0 )
                return startDate;
            return entries[entries.Count - 1];
        }

        public void Reset()
        {
            entries.Clear();
            previousStreak= 0;
        }
    }

    // List of all the Entries
    public class HabitCollection
    {
        public List<Habit> entries = new List<Habit>();
    }


    public class HabitManager
    {
        public HabitCollection Data { get; private set; }
        private bool dirty = false;

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
            dirty = true;
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
            }
        }
        public void Save()
        {
            if (!dirty)
                return;

            string serializedData = JsonConvert.SerializeObject(Data, Formatting.Indented);
            File.WriteAllText(JConstants.HABITS_FILENAME, serializedData);
            dirty = false;

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
            dirty = true;
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


