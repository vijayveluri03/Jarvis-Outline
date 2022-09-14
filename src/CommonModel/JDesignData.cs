using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jarvis
{

    // Customizable options like Look and Feel or other attributes which can be changed through settings. 

    public class JDesignData
    {

        [Serializable]
        public class LookAndFeelProperties
        {
            [JsonProperty] public string defaultColorForText = "White";
            [JsonProperty] public string highlightColorForText = "White";
            [JsonProperty] public string highlightColorForText_2 = "White";
            [JsonProperty] public string highlightColorForText_3 = "White";
            [JsonProperty] public string highlightColorForText_4 = "White";
            [JsonProperty] public string highlightColorForText_5 = "White";
            [JsonProperty] public string highlightColorForText_disabled = "White";
        }
        [Serializable]
        public class Categories
        {
            [JsonProperty] public string[] listOfCategories;
        }

        public static JDesignData instance { get; private set; }

        [JsonProperty] public LookAndFeelProperties lookAndFeel;
        [JsonProperty] public Categories categories;
        [JsonProperty] public string defaultExternalEditor = "code";

        [JsonProperty] public Dictionary<string, string> properties = new Dictionary<string, string>();    

        // Getters        
        [JsonIgnore] public ConsoleColor DefaultColorForText { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.defaultColorForText); } } // used for all default text
        [JsonIgnore] public ConsoleColor HighlightColorForText { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.highlightColorForText); } } //used for headings 
        [JsonIgnore] public ConsoleColor HighlightColorForText_2 { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.highlightColorForText_2); } } // used to highlight stories
        [JsonIgnore] public ConsoleColor HighlightColorForText_3 { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.highlightColorForText_3); } } // used to highlight collections 
        [JsonIgnore] public ConsoleColor HighlightColorForText_4 { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.highlightColorForText_4); } } // unused
        [JsonIgnore] public ConsoleColor HighlightColorForText_5 { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.highlightColorForText_5); } } // unused
        [JsonIgnore] public ConsoleColor HighlightColorForText_Disabled { get { return Utils.ParseEnum<ConsoleColor>(lookAndFeel.highlightColorForText_disabled); } }    // used to represent disabled or inactive stuff


        public static JDesignData Load()
        {
            if (Utils.CreateFileIfNotExit(JConstants.DESIGNDATA_FILENAME, JConstants.DESIGNDATA_TEMPLATE_FILENAME))
            {
                ConsoleWriter.Print("Design config copied from Template. This happens on the first launch.");
            }

            using (StreamReader r = new StreamReader(JConstants.DESIGNDATA_FILENAME))
            {
                string json = r.ReadToEnd();
                JDesignData data = JsonConvert.DeserializeObject<JDesignData>(json);
                instance = data;
                instance.PostLoad();
                return data;
            }
        }

        private void PostLoad()
        {
            InitializeTasks();
            InitializeNotebook();
        }

        #region PROPERTIES 

        public bool TryGetProperty(string key, out string value)
        {
            if(properties.ContainsKey(key))
            {
                value = properties[key];
                return true;
            }
            value = null;
            return false;
        }
        public bool TryGetIntegerProperty(string key, out int value)
        {
            if (properties.ContainsKey(key))
            {
                value = Utils.Conversions.Atoi( properties[key]);
                return true;
            }
            value = 0;
            return false;
        }

        public string GetProperty( string key )
        {
            return properties[key];
        }
        public int GetIntegerProperty(string key)
        {
            return Utils.Conversions.Atoi(GetProperty(key));
        }
        public bool GetBoolProperty(string key)
        {
            return Utils.Conversions.Atob(GetProperty(key));
        }

        #endregion // PROPERTIES


        public bool DoesCategoryExist(string category)
        {
            foreach (var cat in categories.listOfCategories)
            {
                if (cat == category)
                    return true;
            }
            return false;
        }


        public bool DoesCategoryExist(string[] categories)
        {
            Utils.Assert(categories.Length > 0);
            foreach (var cat in categories)
            {
                if (!DoesCategoryExist(cat))
                    return false;
            }
            return true;
        }


        #region TASK SPECIFIC

        [Serializable]
        public class Tasks
        {
            [JsonProperty] public string[] statusList;
            [JsonProperty] public string defaultStatus;
            [JsonProperty] public string completeStatus;
            [JsonProperty] public string projectStatus;
            [JsonProperty] public string nextactionStatus;
            [JsonProperty] public string reviewStatus;
            [JsonProperty] public string laterStatus;
        }
        [JsonProperty] public Tasks tasks;
        [JsonIgnore] private HashSet<string> cachedTaskStatuses = new HashSet<string>();
        [JsonIgnore] public string TaskDefaultStatus { get { return tasks.defaultStatus; } }
        [JsonIgnore] public string TaskCompletedStatus { get { return tasks.completeStatus; } }
        [JsonIgnore] public string TaskProjectStatus { get { return tasks.projectStatus; } }
        [JsonIgnore] public string TaskNextActionStatus { get { return tasks.nextactionStatus; } }
        [JsonIgnore] public string TaskReviewStatus { get { return tasks.reviewStatus; } }
        [JsonIgnore] public string TaskLaterStatus { get { return tasks.laterStatus; } }


        void InitializeTasks()
        {
            foreach (var status in tasks.statusList)
            {
                if (cachedTaskStatuses.Contains(status))
                {
                    ConsoleWriter.Print("Error : Statuses are repeated in Data/Design.Json");
                    //@todo raise exception ?
                    continue;
                }
                cachedTaskStatuses.Add(status);
            }
            if (!cachedTaskStatuses.Contains(tasks.defaultStatus))
            {
                ConsoleWriter.Print("Error : Default status is invalid in Data/Design.Json. It should be one of the pre defined statuses.");
                //@todo raise exception ?
            }
            if (!cachedTaskStatuses.Contains(tasks.completeStatus))
            {
                ConsoleWriter.Print("Error : Complete status is invalid in Data/Design.Json. It should be one of the pre defined statuses.");
                //@todo raise exception ?
            }

        }

        public bool DoesTaskStatusExist(string status)
        {
            return cachedTaskStatuses.Contains(status);
        }

        public bool DoesTaskStatusExistFuzzySearch(string status)
        {
            status = status.ToLower();
            foreach (var st in cachedTaskStatuses)
            {
                if (st.ToLower().Contains(status))
                {
                    return true;
                }
            }
            return false;
        }



        #endregion TASK SPECIFIC

        #region JOURNAL SPECIFIC

        [Serializable]
        public class Notebook
        {
            [JsonProperty] public string[] listOfTags;
            [JsonProperty] public string defaultTag;
        }
        [JsonProperty] public Notebook notebook;
        [JsonIgnore] private HashSet<string> cachedJornalTags = new HashSet<string>();
        [JsonIgnore] public string NotebookDefaultTag { get { return notebook.defaultTag; } }


        void InitializeNotebook()
        {
            foreach (var status in notebook.listOfTags)
            {
                if (cachedJornalTags.Contains(status))
                {
                    ConsoleWriter.Print("Error : tags are repeated in Data/Design.Json");
                    //@todo raise exception ?
                    continue;
                }
                cachedJornalTags.Add(status);
            }
            if (!cachedJornalTags.Contains(notebook.defaultTag))
            {
                ConsoleWriter.Print("Error : Default tag is invalid in Data/Design.Json. It should be one of the pre defined tags.");
                //@todo raise exception ?
            }
        }

        public bool DoesNotebookTagExist(string[] tags)
        {
            Utils.Assert(tags.Length > 0);
            foreach (var tag in tags)
            {
                if (!DoesNotebookTagExist(tag))
                    return false;
            }
            return true;
        }

        public bool DoesNotebookTagExist(string tag)
        {
            return cachedJornalTags.Contains(tag);
        }

        public bool DoesNotebookTagExistFuzzySearch(string tag)
        {
            tag = tag.ToLower();
            foreach (var st in cachedJornalTags)
            {
                if (st.ToLower().Contains(tag))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion JOURNAL SPECIFIC 

    }


}
