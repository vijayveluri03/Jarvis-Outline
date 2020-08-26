using System;
using System.Collections.Generic;
using System.Linq;

namespace Jarvis {
    public class SharedLogic {

        public static SharedLogic Load() {
            SharedLogic instance = new SharedLogic();
            return instance;
        }
        public void PostInit(OutlineManager outlinemanager, PomoManager pomoManager, JUserData userData, JDesignData designData) {
            this.outlineManager = outlinemanager;
            this.pomoManager = pomoManager;
            this.userData = userData;
            this.designData = designData;
        }


        // Create a new Entry (not a task) under a parent.

        public EntryData CreateNewEntry( int parentID ) {
            EntryData ed = new EntryData();
            ed.id = outlineManager.GetNewID();

            ed.title = Utils.GetUserInputString("Enter title:");
            ed.parentID = Utils.GetUserInputInt("Enter parent:", parentID);
            ed.CreatedDate = Utils.Now;

            if (outlineManager.IsEntryAvailableWithID(ed.parentID)) {
                outlineManager.AddEntry(ed);
                return ed;
            }
            else {
                ConsoleWriter.PrintInRed("Parent not found");
                return null;
            }
        }

        // Remove a task and all its Children & Unlink the connections

        public void RemoveTask(EntryData entryData) {
            // Unlink
            if (entryData.links.Count > 0) {
                List<int> links = new List<int>(entryData.links);
                foreach (int linkID in links) {
                    EntryData linkEntryData = outlineManager.GetEntry(linkID);
                    linkEntryData.UnLink(entryData.id);
                    entryData.UnLink(linkEntryData.id);
                }
            }
            // Remove all children recursively
            List<EntryData> entries = outlineManager.outlineData.entries.FindAll(X => (X != null && X.parentID == entryData.id));
            if (entries.Count > 0) {
                foreach (EntryData child in entries) {
                    RemoveTask(child);
                    outlineManager.RemoveEntry(child);
                }
            }
            outlineManager.RemoveEntry(entryData);
        }

        // Marks a task as complete

        public void CompleteTask(int id) {
            if (IsEntryValidOrPrintError(id)) {
                EntryData ed = this.outlineManager.GetEntry(id);
                if (!ed.IsTask)
                    ConsoleWriter.PrintInRed("This is not a task");
                else
                    ed.SetAsComplete(Utils.Now);
            }
            else
                ConsoleWriter.PrintInRed("Action not complete");
        }

        // Clones. Clones the links and the parent.
        // But children are not cloned.

        public void CloneAnEntry(EntryData entryData) {
            EntryData clonesTask = new EntryData();
            clonesTask.id = outlineManager.GetNewID();

            clonesTask.title = entryData.title;
            clonesTask.parentID = entryData.parentID;
            clonesTask.CreatedDate = Utils.Now;
            if (entryData.IsTask) {
                DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                clonesTask.SetAsTask(dueDate);
            }

            outlineManager.AddEntry(clonesTask);

            if (entryData.links.Count > 0) {
                foreach (int linkID in entryData.links) {
                    EntryData linkEntryData = outlineManager.GetEntry(linkID);
                    linkEntryData.AddLink(clonesTask.id);
                    clonesTask.AddLink(linkEntryData.id);
                }
            }
        }

        public void SaveAll() {
            outlineManager.Save();
            pomoManager.Save();
            userData.Save();
        }

        public bool IsEntryValidOrPrintError(int id) {
            if (outlineManager.IsEntryAvailableWithID(id)) {
                return true;
            }
            else
                ConsoleWriter.PrintInRed("Parent not found");
            return false;
        }



        // UI logic - Hot keys for actions to be performed.
        // Along with the logic needed to be executed for a Hot key

        #region ACTION PARAMS LOGIC

        // Context required for the shared logic to access specific information
        public class OutlineMenuActionParamsContext : Utils.IActionParamsContext {

            public int parentID;
            public static List<int> pomoLinks = new List<int>();
        };

        public Utils.ActionParams CreateActionParamsForNewEntry (OutlineMenuActionParamsContext contextAtt) {

            Utils.ActionParams ap = new Utils.ActionParams( "a", "a. Add new entry", delegate (Utils.IActionParamsContext contextNew) {
                Utils.Assert(contextNew != null && contextNew is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = contextNew as OutlineMenuActionParamsContext;

                if (CreateNewEntry( (contextNew as OutlineMenuActionParamsContext).parentID ) != null)
                    ConsoleWriter.Print("Added!");
                else
                    ConsoleWriter.PrintInRed("Not Added");
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsForTaskCompletion(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "c", "c. Complete", delegate (Utils.IActionParamsContext contextNew) {
                Utils.Assert(contextNew != null && contextNew is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = contextNew as OutlineMenuActionParamsContext;

                string commaSeperatedID = Utils.GetUserInputString("Entry ID (comma seperated):", contextOutlineMenu.parentID.ToString());
                int[] IDs = Utils.ConvertCommaAndHyphenSeperateStringToIDs(commaSeperatedID);
                foreach (int id in IDs) {
                    CompleteTask(id);
                }
            });
            ap.context = contextAtt;
            return ap;

        }
        public Utils.ActionParams CreateActionParamsToDiscardTask (OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "d", "d. Discard", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                int id = Utils.GetUserInputInt("Entry ID:", contextOutlineMenu.parentID);
                if (IsEntryValidOrPrintError(id)) {
                    EntryData ed = this.outlineManager.GetEntry(id);
                    if (!ed.IsTask)
                        ConsoleWriter.PrintInRed("This is not a task");
                    else
                        ed.SetAsDiscarded(Utils.Now);
                }
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToRemoveEntry(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "r", "r. Remove", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                ConsoleWriter.Print("All the links will be broken. and all children will be removed.");
                bool confirmation = Utils.GetConfirmationFromUser("Do you want to proceed", false);

                if (confirmation) {
                    int id = Utils.GetUserInputInt("Entry ID:", contextOutlineMenu.parentID);
                    if (IsEntryValidOrPrintError(id)) {
                        EntryData ed = this.outlineManager.GetEntry(id);
                        RemoveTask(ed);
                    }
                }
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToRefresh(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "refresh", "refresh. Refresh", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToLinkTwoEntries(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "l", "l. link to Task", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                int id = Utils.GetUserInputInt("Entry ID:", contextOutlineMenu.parentID);
                if (IsEntryValidOrPrintError(id)) {
                    int id2 = Utils.GetUserInputInt("Entry 2 ID:");
                    if (IsEntryValidOrPrintError(id2)) {
                        EntryData ed1 = this.outlineManager.GetEntry(id);
                        EntryData ed2 = this.outlineManager.GetEntry(id2);

                        ed1.AddLink(ed2.id);
                        ed2.AddLink(ed1.id);
                    }
                }
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToCreateANewTask(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "at", "at. Create a Task", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                EntryData ed = CreateNewEntry(contextOutlineMenu.parentID);
                DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                ed.SetAsTask(dueDate);

                ConsoleWriter.Print("Converted to task and date set to today!");
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToConvertToATask(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "t", "t. Convert to Task", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                int id = Utils.GetUserInputInt("Entry ID:", contextOutlineMenu.parentID);
                if (IsEntryValidOrPrintError(id)) {
                    DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                    this.outlineManager.GetEntry(id).SetAsTask(dueDate);
                }
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToEditAnEntry(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "e", "e. Edit", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                int id = Utils.GetUserInputInt("Entry ID:", contextOutlineMenu.parentID);
                if (IsEntryValidOrPrintError(id)) {
                    EntryData ed = this.outlineManager.GetEntry(id);

                    if (ed.IsTask && !ed.IsTaskClosed) {
                        ed.TaskDueDate = Utils.GetDateFromUser("Due Date", ed.TaskDueDate);
                    }
                    ed.title = Utils.GetUserInputString("Enter Title:", ed.title);
                }
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToConvertToAnEntry(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "-t", "-t. Convert to Note", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                int id = Utils.GetUserInputInt("Entry ID:", contextOutlineMenu.parentID);
                if (IsEntryValidOrPrintError(id)) {
                    //DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                    this.outlineManager.GetEntry(id).ConvertToNotes();
                }
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToChangeParent(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams("p", "p. Change parent", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                int id = Utils.GetUserInputInt("Entry ID:", contextOutlineMenu.parentID);
                if (IsEntryValidOrPrintError(id)) {
                    EntryData ed = outlineManager.GetEntry(id);
                    int parentID = Utils.GetUserInputInt("Parend ID:");
                    if ( IsEntryValidOrPrintError(parentID)) {
                        ed.parentID = parentID;
                        ConsoleWriter.Print("Parent changed");
                    }
                }
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToCreateATaskAndLinkItToParent(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "atl", "atl. Create a Task and link to parent", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                if (IsEntryValidOrPrintError(contextOutlineMenu.parentID)) {
                    EntryData ed = CreateNewEntry(contextOutlineMenu.parentID);
                    if (ed != null) {
                        DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                        ed.SetAsTask(dueDate);
                        EntryData ed2 = this.outlineManager.GetEntry(contextOutlineMenu.parentID);
                        ed.AddLink(ed2.id);
                        ed2.AddLink(ed.id);
                    }
                }
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToCloneAnEntry(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "clone", "clone. Clone", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                ConsoleWriter.Print("Children will not be cloned. links will be maintained!");
                bool confirmation = true; // Utils.GetConfirmationFromUser("Do you want to proceed", false);

                if (confirmation) {
                    string commaSeperatedID = Utils.GetUserInputString("Entry ID (comma seperated):", contextOutlineMenu.parentID.ToString());
                    int[] IDs = Utils.ConvertCommaAndHyphenSeperateStringToIDs(commaSeperatedID);
                    foreach (int id in IDs) {
                        if ( IsEntryValidOrPrintError(id)) {
                            EntryData ed = this.outlineManager.GetEntry(id);
                            CloneAnEntry(ed);
                        }
                        else
                            ConsoleWriter.PrintInRed("Action not performed");
                    }
                }
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToCloneGroup(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "clonegroup", "clonegroup. Clone a group", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                String groupName = Utils.SelectFrom("Groups", "Select group", "habits", "habits", "dailys");

                bool confirmation = Utils.GetConfirmationFromUser("Do you want to proceed", false);

                if (confirmation) {
                    if (groupName == "habits")
                        CloneHabits();
                    else if (groupName == "dailys")
                        CloneTodays();
                    else
                        Utils.Assert(false);
                }
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToStartAPomodoroTimer(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "pomostart", "pomostart. ", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                String timerType = Utils.SelectFrom("Timer type", "Select timer", "MID", "SMALL", "MID", "BIG", "HUGE");
                ePomoTimer timer;
                if (Enum.TryParse(timerType, out timer)) {
                    pomoManager.StartNewPomoTimer(timer);
                }
                else
                    ConsoleWriter.PrintInRed("Failed to create!");
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToLinkTasksToAPomodoroTimer(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "pomolink", "pomolink. ", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                int id = Utils.GetUserInputInt("Entry ID:", contextOutlineMenu.parentID);
                if (IsEntryValidOrPrintError(id)) {
                    if (!OutlineMenuActionParamsContext.pomoLinks.Contains(id))
                        OutlineMenuActionParamsContext.pomoLinks.Add(id);
                }
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToEndPomodoroTimer(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "pomoend", "pomoend. ", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                if (!pomoManager.IsActiveTimerEnded()) {
                    ConsoleWriter.PrintInRed("Pomo timer hasnt ended!");
                    return;
                }
                if (OutlineMenuActionParamsContext.pomoLinks.Count == 0) {
                    ConsoleWriter.Print("We have no pomos linked");
                }
                if (OutlineMenuActionParamsContext.pomoLinks.Count > 0) {
                    ConsoleWriter.PrintWithOutLineBreak("We have these tasks linked:");
                    foreach (int id in OutlineMenuActionParamsContext.pomoLinks)
                        ConsoleWriter.PrintWithOutLineBreak(id + "\t");
                    ConsoleWriter.PrintNewLine();
                }

                bool confirmation = Utils.GetConfirmationFromUser("Is that it ?");

                if (confirmation) {
                    pomoManager.EndTimer(OutlineMenuActionParamsContext.pomoLinks);
                    OutlineMenuActionParamsContext.pomoLinks.Clear();
                }
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToPrintPomodoroStatus(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "pomoprint", "pomoprint. ", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                ConsoleWriter.PushColor(ConsoleColor.Yellow);
                ConsoleWriter.Print("Pomos completed today:" + pomoManager.PomosCountToday());
                ConsoleWriter.PopColor();

                if (OutlineMenuActionParamsContext.pomoLinks.Count == 0) {
                    ConsoleWriter.Print("We have no pomos linked");
                }
                if (OutlineMenuActionParamsContext.pomoLinks.Count > 0) {
                    ConsoleWriter.PrintWithOutLineBreak("We have these tasks linked:");
                    foreach (int id in OutlineMenuActionParamsContext.pomoLinks)
                        ConsoleWriter.PrintWithOutLineBreak(id + "\t");
                    ConsoleWriter.PrintNewLine();
                }
            });
            ap.context = contextAtt;
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToSaveAll(OutlineMenuActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "s", "s. save", delegate (Utils.IActionParamsContext context) {
                Utils.Assert(context != null && context is OutlineMenuActionParamsContext);
                OutlineMenuActionParamsContext contextOutlineMenu = context as OutlineMenuActionParamsContext;

                SaveAll();
            });
            ap.context = contextAtt;
            return ap;
        }


        // @todo - Hardcoded - Remove these 
        private void CloneHabits() {
            int[] taskIDsToClone = new int[] { 86, 88, 89, 90, 91, 92, 95, 96, 192, 385, 336 };

            foreach (int habitTaskID in taskIDsToClone) {
                EntryData ed = this.outlineManager.GetEntry(habitTaskID);
                CloneAnEntry(ed);
            }
        }
        private void CloneTodays() {
            int[] taskIDsToClone = new int[] { 276, 277, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288, 289, 290, 292, 293 };

            foreach (int habitTaskID in taskIDsToClone) {
                EntryData ed = this.outlineManager.GetEntry(habitTaskID);
                CloneAnEntry(ed);
            }
        }

        #endregion

        private OutlineManager outlineManager;
        private PomoManager pomoManager;
        private JUserData userData;
        private JDesignData designData;

    }
}
