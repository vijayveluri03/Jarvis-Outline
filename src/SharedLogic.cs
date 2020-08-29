using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

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
        public class ActionParamsContext : Utils.aActionParamsContext {

            public int parentIDForNewTasks;
            public static List<int> pomoLinks = new List<int>();
        };

        public Utils.ActionParams CreateActionParamsForNewEntry (ActionParamsContext contextAtt) {

            Utils.ActionParams ap = new Utils.ActionParams( "a", "a. Add new entry", delegate (Utils.aActionParamsContext contextNew) {
                Utils.Assert(contextNew != null && contextNew is ActionParamsContext);
                ActionParamsContext contextOutlineMenu = contextNew as ActionParamsContext;

                if (CreateNewEntry( (contextNew as ActionParamsContext).parentIDForNewTasks ) != null)
                    ConsoleWriter.Print("Added!");
                else
                    ConsoleWriter.PrintInRed("Not Added");
            });
            ap.SetContext(contextAtt);
            return ap;
        }
        public Utils.ActionParams CreateActionParamsForTaskCompletion() {
            Utils.ActionParams ap = new Utils.ActionParams( "c", "c. Complete", delegate (Utils.aActionParamsContext contextNew) {
                string commaSeperatedID = Utils.GetUserInputString("Entry ID (comma seperated):");
                int[] IDs = Utils.ConvertCommaAndHyphenSeperateStringToIDs(commaSeperatedID);
                foreach (int id in IDs) {
                    if (IsEntryValidOrPrintError(id))
                        CompleteTask(id);
                }
            });
            return ap;

        }
        public Utils.ActionParams CreateActionParamsToDiscardTask () {
            Utils.ActionParams ap = new Utils.ActionParams( "d", "d. Discard", delegate (Utils.aActionParamsContext context) {
                string commaSeperatedID = Utils.GetUserInputString("Entry ID (comma seperated):");
                int[] IDs = Utils.ConvertCommaAndHyphenSeperateStringToIDs(commaSeperatedID);
                foreach (int id in IDs) {
                    if (IsEntryValidOrPrintError(id)) {
                        EntryData ed = this.outlineManager.GetEntry(id);
                        if (!ed.IsTask)
                            ConsoleWriter.PrintInRed("This is not a task");
                        else
                            ed.SetAsDiscarded(Utils.Now);
                    }
                }
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToRemoveEntry() {
            Utils.ActionParams ap = new Utils.ActionParams( "r", "r. Remove", delegate (Utils.aActionParamsContext context) {
                ConsoleWriter.Print("All the links will be broken. and all children will be removed.");
                bool confirmation = true;// Utils.GetConfirmationFromUser("Do you want to proceed", false);

                if (confirmation) {
                    string commaSeperatedID = Utils.GetUserInputString("Entry ID (comma seperated):");
                    int[] IDs = Utils.ConvertCommaAndHyphenSeperateStringToIDs(commaSeperatedID);
                    foreach (int id in IDs) {
                        if (IsEntryValidOrPrintError(id)) {
                            EntryData ed = this.outlineManager.GetEntry(id);
                            RemoveTask(ed);
                        }
                    }
                }
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToRefresh() {
            Utils.ActionParams ap = new Utils.ActionParams( "refresh", "refresh. Refresh", delegate (Utils.aActionParamsContext context) {
                
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToLinkTwoEntries() {
            Utils.ActionParams ap = new Utils.ActionParams("l", "l. link to Task", delegate (Utils.aActionParamsContext context) {
                string commaSeperatedID = Utils.GetUserInputString("Entry ID (comma seperated):");
                int id2 = Utils.GetUserInputInt("Entry 2 ID:");
                int[] IDs = Utils.ConvertCommaAndHyphenSeperateStringToIDs(commaSeperatedID);
                foreach (int id in IDs) {
                    if (IsEntryValidOrPrintError(id)) {
                        if (IsEntryValidOrPrintError(id2)) {
                            EntryData ed1 = this.outlineManager.GetEntry(id);
                            EntryData ed2 = this.outlineManager.GetEntry(id2);

                            ed1.AddLink(ed2.id);
                            ed2.AddLink(ed1.id);
                        }
                    }
                }
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToUnLinkTwoEntries() {
            Utils.ActionParams ap = new Utils.ActionParams("-l", "-l. unlink", delegate (Utils.aActionParamsContext context) {
                string commaSeperatedID = Utils.GetUserInputString("Entry ID (comma seperated):");
                int id2 = Utils.GetUserInputInt("Entry 2 ID:");
                int[] IDs = Utils.ConvertCommaAndHyphenSeperateStringToIDs(commaSeperatedID);
                foreach (int id in IDs) {
                    if (IsEntryValidOrPrintError(id)) {
                        if (IsEntryValidOrPrintError(id2)) {
                            EntryData ed1 = this.outlineManager.GetEntry(id);
                            EntryData ed2 = this.outlineManager.GetEntry(id2);

                            ed1.UnLink(ed2.id);
                            ed2.UnLink(ed1.id);
                        }
                    }
                }
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToCreateANewTask(ActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "at", "at. Create a Task", delegate (Utils.aActionParamsContext context) {
                Utils.Assert(context != null && context is ActionParamsContext);
                ActionParamsContext contextOutlineMenu = context as ActionParamsContext;

                EntryData ed = CreateNewEntry(contextOutlineMenu.parentIDForNewTasks);
                DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                ed.SetAsTask(dueDate);

                ConsoleWriter.Print("Converted to task and date set to today!");
            });
            ap.SetContext(contextAtt);
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToConvertToATask() {
            Utils.ActionParams ap = new Utils.ActionParams( "t", "t. Convert to Task", delegate (Utils.aActionParamsContext context) {
                int id = Utils.GetUserInputInt("Entry ID:");
                if (IsEntryValidOrPrintError(id)) {
                    DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                    this.outlineManager.GetEntry(id).SetAsTask(dueDate);
                }
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToEditAnEntry() {
            Utils.ActionParams ap = new Utils.ActionParams( "e", "e. Edit", delegate (Utils.aActionParamsContext context) {
                int id = Utils.GetUserInputInt("Entry ID:");
                if (IsEntryValidOrPrintError(id)) {
                    EntryData ed = this.outlineManager.GetEntry(id);

                    if (ed.IsTask && !ed.IsTaskClosed) {
                        ed.TaskDueDate = Utils.GetDateFromUser("Due Date", ed.TaskDueDate);
                    }
                    ed.title = Utils.GetUserInputString("Enter Title:", ed.title);
                }
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToConvertToAnEntry() {
            Utils.ActionParams ap = new Utils.ActionParams( "-t", "-t. Convert to Note", delegate (Utils.aActionParamsContext context) {
                int id = Utils.GetUserInputInt("Entry ID:");
                if (IsEntryValidOrPrintError(id)) {
                    //DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                    this.outlineManager.GetEntry(id).ConvertToEntry();
                }
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToChangeParent() {
            Utils.ActionParams ap = new Utils.ActionParams("p", "p. Change parent", delegate (Utils.aActionParamsContext context) {
            string commaSeperatedID = Utils.GetUserInputString("Entry ID (comma seperated):");
                int parentID = Utils.GetUserInputInt("Parend ID:");
                int[] IDs = Utils.ConvertCommaAndHyphenSeperateStringToIDs(commaSeperatedID);
                foreach (int id in IDs) {
                    if (IsEntryValidOrPrintError(id)) {
                        EntryData ed = outlineManager.GetEntry(id);
                        if (IsEntryValidOrPrintError(parentID)) {
                            ed.parentID = parentID;
                            ConsoleWriter.Print("Parent changed");
                        }
                    }
                }
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToCreateATaskAndLinkItToParent(ActionParamsContext contextAtt) {
            Utils.ActionParams ap = new Utils.ActionParams( "atl", "atl. Create a Task and link to parent", delegate (Utils.aActionParamsContext context) {
                Utils.Assert(context != null && context is ActionParamsContext);
                ActionParamsContext contextOutlineMenu = context as ActionParamsContext;

                if (IsEntryValidOrPrintError(contextOutlineMenu.parentIDForNewTasks)) {
                    EntryData ed = CreateNewEntry(contextOutlineMenu.parentIDForNewTasks);
                    if (ed != null) {
                        DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                        ed.SetAsTask(dueDate);
                        EntryData ed2 = this.outlineManager.GetEntry(contextOutlineMenu.parentIDForNewTasks);
                        ed.AddLink(ed2.id);
                        ed2.AddLink(ed.id);
                    }
                }
            });
            ap.SetContext(contextAtt);
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToCloneAnEntry() {
            Utils.ActionParams ap = new Utils.ActionParams( "clone", "clone. Clone", delegate (Utils.aActionParamsContext context) {
                ConsoleWriter.Print("Children will not be cloned. links will be maintained!");
                bool confirmation = true; // Utils.GetConfirmationFromUser("Do you want to proceed", false);

                if (confirmation) {
                    string commaSeperatedID = Utils.GetUserInputString("Entry ID (comma seperated):");
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
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToMarkATask() {
            return new Utils.ActionParams("mark", "mark. Toggle Mark an entry", delegate (Utils.aActionParamsContext context) {
                string commaSeperatedID = Utils.GetUserInputString("Entry ID (comma seperated):");
                int[] IDs = Utils.ConvertCommaAndHyphenSeperateStringToIDs(commaSeperatedID);
                foreach (int id in IDs) {
                    if (IsEntryValidOrPrintError(id)) {
                        if (!userData.markedTaskIDs.Contains(id)) {
                            userData.markedTaskIDs.Add(id);
                            ConsoleWriter.Print("Marked!");
                        }
                        else {
                            userData.markedTaskIDs.Remove(id);
                            ConsoleWriter.Print("Un Marked!");
                        }
                    }
                }
            });
        }
        public Utils.ActionParams CreateActionParamsToPinAnEntry() {
            return new Utils.ActionParams("pin", "pin. Toggle Pin an entry", delegate (Utils.aActionParamsContext context) {
                string commaSeperatedID = Utils.GetUserInputString("Entry ID (comma seperated):");
                int[] IDs = Utils.ConvertCommaAndHyphenSeperateStringToIDs(commaSeperatedID);
                foreach (int id in IDs) {
                    if (IsEntryValidOrPrintError(id)) {
                        if (!userData.pinnedTaskIDs.Contains(id)) {
                            userData.pinnedTaskIDs.Add(id);
                            ConsoleWriter.Print("Pinned!");
                        }
                        else {
                            userData.pinnedTaskIDs.Remove(id);
                            ConsoleWriter.Print("Un Pinned!");
                        }
                    }
                }
            });
        }
        public Utils.ActionParams CreateActionParamsToShowAllCommands(Utils.aActionParamsContext contextAtt) {
            return new Utils.ActionParams("misc", "misc. Show all commands", delegate (Utils.aActionParamsContext context) {
                Utils.Assert(context != null );

                //@todo - some dirty code. Clean it.
                Utils.aActionParamsContext.DisplayAllCommands = !Utils.aActionParamsContext.DisplayAllCommands;

                if (Utils.aActionParamsContext.DisplayAllCommands)
                    ConsoleWriter.Print("Displaying all commands!");
                else
                    ConsoleWriter.Print("Hiding some commands!");
            }).SetContext(contextAtt) ;
        }
        public Utils.ActionParams CreateActionParamsToCloneGroup() {
            Utils.ActionParams ap = new Utils.ActionParams( "clonegroup", "clonegroup. Clone a group", delegate (Utils.aActionParamsContext context) {
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
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToStartAPomodoroTimer() {
            Utils.ActionParams ap = new Utils.ActionParams( "pomostart", "pomostart. ", delegate (Utils.aActionParamsContext context) {
                String timerType = Utils.SelectFrom("Timer type", "Select timer", "MID", "SMALL", "MID", "BIG", "HUGE");
                ePomoTimer timer;
                if (Enum.TryParse(timerType, out timer)) {
                    pomoManager.StartNewPomoTimer(timer);
                }
                else
                    ConsoleWriter.PrintInRed("Failed to create!");
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToLinkTasksToAPomodoroTimer() {
            Utils.ActionParams ap = new Utils.ActionParams( "pomolink", "pomolink. ", delegate (Utils.aActionParamsContext context) {
                int id = Utils.GetUserInputInt("Entry ID:");
                if (IsEntryValidOrPrintError(id)) {
                    if (!ActionParamsContext.pomoLinks.Contains(id))
                        ActionParamsContext.pomoLinks.Add(id);
                }
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToEndPomodoroTimer() {
            Utils.ActionParams ap = new Utils.ActionParams( "pomoend", "pomoend. ", delegate (Utils.aActionParamsContext context) {
                if (!pomoManager.IsActiveTimerEnded()) {
                    ConsoleWriter.PrintInRed("Pomo timer hasnt ended!");
                    return;
                }
                if (ActionParamsContext.pomoLinks.Count == 0) {
                    ConsoleWriter.Print("We have no pomos linked");
                }
                if (ActionParamsContext.pomoLinks.Count > 0) {
                    ConsoleWriter.PrintWithOutLineBreak("We have these tasks linked:");
                    foreach (int id in ActionParamsContext.pomoLinks)
                        ConsoleWriter.PrintWithOutLineBreak(id + "\t");
                    ConsoleWriter.PrintNewLine();
                }

                bool confirmation = Utils.GetConfirmationFromUser("Is that it ?");

                if (confirmation) {
                    pomoManager.EndTimer(ActionParamsContext.pomoLinks);
                    ActionParamsContext.pomoLinks.Clear();
                }
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToPrintPomodoroStatus() {
            Utils.ActionParams ap = new Utils.ActionParams( "pomoprint", "pomoprint. ", delegate (Utils.aActionParamsContext context) {
                ConsoleWriter.PushColor(ConsoleColor.Yellow);
                ConsoleWriter.Print("Pomos completed today:" + pomoManager.PomosCountToday());
                ConsoleWriter.PopColor();

                if (ActionParamsContext.pomoLinks.Count == 0) {
                    ConsoleWriter.Print("We have no pomos linked");
                }
                if (ActionParamsContext.pomoLinks.Count > 0) {
                    ConsoleWriter.PrintWithOutLineBreak("We have these tasks linked:");
                    foreach (int id in ActionParamsContext.pomoLinks)
                        ConsoleWriter.PrintWithOutLineBreak(id + "\t");
                    ConsoleWriter.PrintNewLine();
                }
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToSaveAll() {
            Utils.ActionParams ap = new Utils.ActionParams( "s", "s. save", delegate (Utils.aActionParamsContext context) {
                SaveAll();
            });
            return ap;
        }


        public Utils.ActionParams CreateActionParamsToSetURL() {
            Utils.ActionParams ap = new Utils.ActionParams("seturl", "seturl", delegate (Utils.aActionParamsContext context) {
                int id = Utils.GetUserInputInt("Entry ID:");
                if (IsEntryValidOrPrintError(id)) {
                    //DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                    string url = Utils.GetUserInputString("Enter URL:");
                    this.outlineManager.GetEntry(id).URLToOpen = url;
                }
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToClearURL() {
            Utils.ActionParams ap = new Utils.ActionParams("clearurl", "clearurl", delegate (Utils.aActionParamsContext context) {
                int id = Utils.GetUserInputInt("Entry ID:");
                if (IsEntryValidOrPrintError(id)) {
                    //DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                    this.outlineManager.GetEntry(id).URLToOpen = "";
                }
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToPrintURL() {
            Utils.ActionParams ap = new Utils.ActionParams("printurl", "printurl", delegate (Utils.aActionParamsContext context) {
                int id = Utils.GetUserInputInt("Entry ID:");
                if (IsEntryValidOrPrintError(id)) {
                    ConsoleWriter.Print("URL is " + this.outlineManager.GetEntry(id).URLToOpen);
                }
            });
            return ap;
        }
        public Utils.ActionParams CreateActionParamsToOpenURL() {
            Utils.ActionParams ap = new Utils.ActionParams("openurl", "openurl", delegate (Utils.aActionParamsContext context) {
                int id = Utils.GetUserInputInt("Entry ID:");
                if (IsEntryValidOrPrintError(id)) {
                    EntryData ed = this.outlineManager.GetEntry(id);
                    if (ed.DoesUrlExist) {
                        ConsoleWriter.PrintURL("Opening " + ed.URLToOpen);
                        // if mac
                        string command = "open \'" + ed.URLToOpen + "\'";
                        // else @todo
                        Utils.ExecuteCommandInConsole(command);
                    }
                    else
                        ConsoleWriter.Print("No URL present");
                }
            });
            return ap;
        }
        

        // @todo - Hardcoded - Remove these 
        private void CloneHabits() {
            int[] taskIDsToClone = new int[] { 86, 88, 89, 90, 91, 92, 95, 96, 192, 385, 531 };

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
