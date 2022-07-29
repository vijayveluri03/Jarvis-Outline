using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jarvis
{
    public class NotebookEntry
    {
        // The name or the title for an entry 
        public string title;

        public string[] tags;

        public DateTime loggedDate;

        // Unique ID
        public int id;

        public string TagsString
        {
            get
            {
                string tagsString = "";
                foreach( var tag in tags )
                {
                    tagsString += tag + ",";
                }
                return tagsString;
            }
        }
        public bool HasTag(string searchTag, bool fuzzySearch)
        {
            if (tags == null || tags.Length == 0)
                return false;
            foreach(var tag in tags)
            {
                if(fuzzySearch)
                {
                    if (tag.Contains(searchTag))
                        return true;
                }
                else
                {
                    if (tag == searchTag)
                        return true;
                }
            }
            return false;
        }
    }

    // List of all the Entries
    public class NotebookCollection
    {
        public List<NotebookEntry> entries = new List<NotebookEntry>();
    }


    public class NotebookManager : IDirtyable
    {
        public NotebookCollection Data { get; private set; }
        public NotebookManager(string defaultTag, System.Func<string, bool> isTagValid)
        {
            if (Utils.CreateFileIfNotExit(JConstants.JOURNAL_FILENAME, JConstants.JOURNAL_TEMPLATE_FILENAME))
            {
                ConsoleWriter.Print("Notebooks data copied from Template. This happens on the first launch.");
            }

            Load(JConstants.JOURNAL_FILENAME, defaultTag, isTagValid);
        }

        public void AddNotebookEntry(NotebookEntry ed)
        {
            Data.entries.Add(ed);
            IsDirty = true;
        }

        private void Load(string fileName, string defaultTag, System.Func<string, bool> isTagValid)
        {
            using (StreamReader r = new StreamReader(fileName))
            {
                string json = r.ReadToEnd();
                NotebookCollection data = JsonConvert.DeserializeObject<NotebookCollection>(json);
                Data = data;
            }

            foreach (NotebookEntry ed in Data.entries)
            {
                if (ed.id >= availableID)
                    availableID = ed.id + 1;
            }

            PostLoad(defaultTag, isTagValid);
        }

        private void PostLoad(string defaultTag, System.Func<string, bool> isTagValid)
        {
            // This is for data migration for older versions or from previous statuses to the new ones.
            
            foreach (var ed in Data.entries)
            {
                List<string> tags = ed.tags == null ? new List<string>( ) : new List<string>(ed.tags);

                List<string> validTags =    new List<string>();
                foreach (var tag in tags)
                {
                    if (tag.IsEmpty() || !isTagValid(tag))
                    {
                        IsDirty = true;
                    }
                    else
                        validTags.Add(tag);
                }

                if (validTags == null || validTags.Count == 0)
                {
                    IsDirty = true;
                    validTags.Add(defaultTag);
                }

                ed.tags = validTags.ToArray();  
            }
        }


        public void Save()
        {
            if (!IsDirty)
                return;

            string serializedData = JsonConvert.SerializeObject(Data, Formatting.Indented);
            File.WriteAllText(JConstants.JOURNAL_FILENAME, serializedData);
            IsDirty = false;

#if RELEASE_LOG
            ConsoleWriter.Print("Notebooks saved");
#endif
        }

        // Getters

        public bool DoesNotebookEntryExist(int id)
        {
            foreach (NotebookEntry ed in Data.entries)
            {
                if (ed.id == id)
                    return true;
            }
            return false;
        }

        // Because C# doesnt in const (-|-)
        public NotebookEntry GetNotebookEntry_ReadOnly(int id)
        {
            foreach (NotebookEntry ed in Data.entries)
            {
                if (ed.id == id)
                    return ed;
            }
            return null;
        }

        public NotebookEntry GetNotebookEntry_Editable(int id)
        {
            IsDirty = true;
            return GetNotebookEntry_ReadOnly(id);
        }

        public bool RemoveNotebookEntry(NotebookEntry ed)
        {
            IsDirty = true;
            return Data.entries.Remove(ed);
        }
        public bool RemoveNotebookEntryIfExists(int id)
        {
            IsDirty = true;

            var ed = GetNotebookEntry_Editable(id);
            if (ed != null)
                return Data.entries.Remove(ed);
            return false;
        }


        public int GetAvailableID()
        {
            return availableID++;
        }

        // Private 
        private int availableID = 1;
    }

}


