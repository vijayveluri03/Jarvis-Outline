using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jarvis
{
    public class Pomo : IDirtyable
    {
        [JsonProperty(PropertyName = "Date")]
        public Date _date;

        [JsonProperty(PropertyName = "entries")]
        public Dictionary<string,int> _entries = new Dictionary<string,int>();

        public void Init()
        {
        }

        public void AddNewEntry(string category, int pomoCount)
        {
            if( !_entries.ContainsKey(category) )
                _entries.Add(category, 0);
            _entries[category] += pomoCount;
            IsDirty = true;
        }

        public void Reset()
        {
            _entries.Clear();

            IsDirty = true;
        }
    }

    // List of all the Entries
    public class PomoCollection
    {
        public List<Pomo> entries = new List<Pomo>();
    }


    public class PomoManager : IDirtyable
    {
        public PomoCollection Data { get; private set; }
        
        public PomoManager()
        {
            if (Utils.CreateFileIfNotExit(JConstants.POMODORO_FILENAME, JConstants.POMODORO_TEMPLATE_FILENAME))
            {
                ConsoleWriter.Print("Pomos data copied from Template. This happens on the first launch.");
            }

            Load(JConstants.POMODORO_FILENAME);
        }

        public void CreatePomoForTodayIfNecessary()
        {
            if( DoesPomoExist(Date.Today))
            {
                return;
            }
            Pomo entry = new Pomo();
            entry._date = Date.Today;

            Data.entries.Add(entry);
            IsDirty = true;
        }

        public void RemovePomo(Pomo ed)
        {
            Data.entries.Remove(ed);
            IsDirty = true;
        }

        private void Load(string fileName)
        {
            using (StreamReader r = new StreamReader(fileName))
            {
                string json = r.ReadToEnd();
                PomoCollection data = JsonConvert.DeserializeObject<PomoCollection>(json);
                Data = data;
            }

            foreach (Pomo ed in Data.entries)
            {
                ed.Init();
            }
        }

        private bool IsDirtyRecursive()
        {
            foreach (Pomo ed in Data.entries)
            {
                if (ed.IsDirty)
                    return true;
            }
            return IsDirty;
        }
        private void UnSetDirtyRecursively()
        {
            foreach (Pomo ed in Data.entries)
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
            File.WriteAllText(JConstants.POMODORO_FILENAME, serializedData);
            UnSetDirtyRecursively();

#if RELEASE_LOG
            ConsoleWriter.Print("Pomos saved");
#endif
        }

        // Getters

        public bool DoesPomoExist(Date date)
        {
            foreach (Pomo ed in Data.entries)
            {
                if (ed._date == date)
                    return true;
            }
            return false;
        }

        // Because C# doesnt in const (-|-)
        public Pomo GetPomo_ReadOnly(Date date)
        {
            foreach (Pomo ed in Data.entries)
            {
                if (ed._date == date)
                    return ed;
            }
            return null;
        }

        public Pomo GetPomo_Editable(Date date)
        {
            IsDirty = true;
            return GetPomo_ReadOnly(date);
        }

    }

}


