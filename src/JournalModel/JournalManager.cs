using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jarvis
{
    public class JournalEntry
    {
        // The name or the title for an entry 
        public string title;

        public DateTime loggedDate;

        // Unique ID
        public int id;
    }

    // List of all the Entries
    public class JournalCollection
    {
        public List<JournalEntry> entries = new List<JournalEntry>();
    }


    public class JournalManager
    {
        public JournalCollection Data { get; private set; }
        private bool dirty = false;

        public JournalManager()
        {
            if (Utils.CreateFileIfNotExit(JConstants.JOURNAL_FILENAME, JConstants.JOURNAL_TEMPLATE_FILENAME))
            {
                ConsoleWriter.Print("Journals data copied from Template. This happens on the first launch.");
            }

            Load(JConstants.JOURNAL_FILENAME);
        }

        public void AddJournal(JournalEntry ed)
        {
            Data.entries.Add(ed);
            dirty = true;
        }

        private void Load(string fileName)
        {
            using (StreamReader r = new StreamReader(fileName))
            {
                string json = r.ReadToEnd();
                JournalCollection data = JsonConvert.DeserializeObject<JournalCollection>(json);
                Data = data;
            }

            foreach (JournalEntry ed in Data.entries)
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
            File.WriteAllText(JConstants.JOURNAL_FILENAME, serializedData);
            dirty = false;

#if RELEASE_LOG
            ConsoleWriter.Print("Journals saved");
#endif
        }

        // Getters

        public bool DoesJournalExist(int id)
        {
            foreach (JournalEntry ed in Data.entries)
            {
                if (ed.id == id)
                    return true;
            }
            return false;
        }

        // Because C# doesnt in const (-|-)
        public JournalEntry GetJournal_ReadOnly(int id)
        {
            foreach (JournalEntry ed in Data.entries)
            {
                if (ed.id == id)
                    return ed;
            }
            return null;
        }

        public JournalEntry GetJournal_Editable(int id)
        {
            dirty = true;
            return GetJournal_ReadOnly(id);
        }

        public int GetAvailableID()
        {
            return availableID++;
        }

        // Private 
        private int availableID = 1;
    }

}


