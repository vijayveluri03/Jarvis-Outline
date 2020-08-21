using System;
using System.Collections.Generic;
using System.Linq;

namespace Jarvis
{
    public class OutlineMenu : QUtils.FSMBaseState
    {
        public class Context
        {
            public Context(JApplication application, int taskid) { this.application = application; this.entryID = taskid; }
            public JApplication application;
            public int entryID;
        };

        public static Context GetContext ( JApplication application, int entryID )
        {
            Context contxt = new Context( application, entryID);
            return contxt;
        }

        public void Exit () 
        {
            application.FSM.Pop();
        }
        public override void OnContext(System.Object context) 
        {
            Utils.Assert(context != null && context is Context, "EntryMenu initialization error");
            application = (context as Context).application;
            parentID = (context as Context).entryID;

            // Making sure the task exists. 
            Utils.Assert(application.OutlineManager.IsEntryAvailableWithID(parentID));
        }

        public override void Update () 
        {
            if ( application.PomoManager.IsThereAnActivePomoTimer() )
            {
                ConsoleWriter.PushColor(ConsoleColor.Yellow);

                if (application.PomoManager.IsActiveTimerEnded())
                    ConsoleWriter.Print("TIMER ENDED");
                else
                    ConsoleWriter.Print("TIMER RUNNING - " + application.PomoManager.GetActiveTime() + " --> " + application.PomoManager.GetTotalTimeToComplete() ); ;

                ConsoleWriter.PopColor();
            }
            ConsoleWriter.PushColor(ConsoleColor.DarkBlue);

            Utils.Assert(application.OutlineManager.IsEntryAvailableWithID(parentID), "Error");
            EntryData entryData = application.OutlineManager.GetEntry(parentID);
            UICommon.PrintEntry(entryData);
            
            ConsoleWriter.PopColor();
            
            List<EntryData> entries = application.OutlineManager.outlineData.entries.FindAll(X => (X != null && X.parentID == parentID ));
            if (sortByDueDate)
                entries = entries.OrderBy(o => o.TaskDueDate).ToList();

            if (entries.Count > 0)
            {
                int lineCount = 0;
                foreach (EntryData entry in entries)
                {
                    if (!hideCompleted || (hideCompleted && !entry.IsTaskClosed))
                    {
                        UICommon.PrintEntryWithColor(entry);
                        lineCount++;
                        if (lineCount == 3)
                        {
                            ConsoleWriter.PrintNewLine();
                            lineCount = 0;
                        }
                    }
                }
            }
            else
            {
                ConsoleWriter.Print("<Empty>");
            }
            
            if (entryData.links.Count > 0)
            {
                ConsoleWriter.Print("+");
                int lineCount = 0;

                foreach (int linkID in entryData.links)
                {
                    EntryData linkEntryData = application.OutlineManager.GetEntry(linkID);
                    if (!hideCompleted || (hideCompleted && !linkEntryData.IsTaskClosed))
                    {
                        if (linkEntryData != null)
                            UICommon.PrintEntryWithColor(linkEntryData);
                        else
                            ConsoleWriter.PrintInRed("ERROR. LINKED TASK DOESNT EXIST:" + linkID);
                        lineCount++;
                        if (lineCount == 3)
                        {
                            ConsoleWriter.PrintNewLine();
                            lineCount = 0;
                        }
                    }
                }
            }



            ConsoleWriter.PrintNewLine();


            Utils.DoAction ( "Outline Menu options:", ":", "refresh", 
            
                new Utils.ActionParams ( true, "a", "a. Add new", delegate(string fullmessage ) 
                {
                    if (CreateNewTask() != null)
                        ConsoleWriter.Print("Added!");
                    else
                        ConsoleWriter.PrintInRed("Not Added");
                }),

                new Utils.ActionParams(true, "j", "j. Jump to", delegate (string fullmessage)
                {
                    int id = Utils.GetUserInputInt("Entry ID:");
                    if (IsEntryValidOrPrintError(id))
                    {
                        application.FSM.PushInNextFrame(new OutlineMenu(), OutlineMenu.GetContext(application, id));
                    }
                }),


                new Utils.ActionParams(true, "c", "c. Complete", delegate (string fullmessage)
                {
                    string commaSeperatedID = Utils.GetUserInputString("Entry ID (comma seperated):", parentID.ToString());
                    string[] IDs = commaSeperatedID.Split(',');
                    foreach (string idStr in IDs)
                    {
                        int id = -1;

                        if (int.TryParse(idStr, out id) && IsEntryValidOrPrintError(id))
                        {
                            EntryData ed = application.OutlineManager.GetEntry(id);
                            if (!ed.IsTask)
                                ConsoleWriter.PrintInRed("This is not a task");
                            else
                                ed.SetAsComplete(Utils.Now);
                        }
                        else
                            ConsoleWriter.PrintInRed("Action not complete");
                    }
                }),
                new Utils.ActionParams(true, "d", "d. Discard", delegate (string fullmessage)
                {
                    int id = Utils.GetUserInputInt("Entry ID:", parentID);
                    if (IsEntryValidOrPrintError(id))
                    {
                        EntryData ed = application.OutlineManager.GetEntry(id);
                        if (!ed.IsTask)
                            ConsoleWriter.PrintInRed("This is not a task");
                        else
                            ed.SetAsDiscarded(Utils.Now);
                    }
                }),

                new Utils.ActionParams(true, "r", "r. Remove", delegate (string fullmessage)
                {
                    ConsoleWriter.Print("All the links will be broken. and all children will be removed.");
                    bool confirmation = Utils.GetConfirmationFromUser("Do you want to proceed", false);

                    if (confirmation)
                    {
                        int id = Utils.GetUserInputInt("Entry ID:", parentID);
                        if (IsEntryValidOrPrintError(id))
                        {
                            EntryData ed = application.OutlineManager.GetEntry(id);
                            RemoveTask(ed);
                        }
                    }
                }),

                new Utils.ActionParams(showAllCommands, "refresh", "refresh. Refresh", delegate (string fullmessage)
                {
                    
                }),


                new Utils.ActionParams(true, "l", "l. link to Task", delegate (string fullmessage)
                {
                    int id = Utils.GetUserInputInt("Entry ID:", parentID);
                    if (IsEntryValidOrPrintError(id))
                    {
                        int id2 = Utils.GetUserInputInt("Entry 2 ID:");
                        if (IsEntryValidOrPrintError(id2))
                        {
                            EntryData ed1 = application.OutlineManager.GetEntry(id);
                            EntryData ed2 = application.OutlineManager.GetEntry(id2);

                            ed1.AddLink(ed2.id);
                            ed2.AddLink(ed1.id);
                        }
                    }
                }),

                 new Utils.ActionParams(true, "at", "at. Create a Task", delegate (string fullmessage)
                 {
                     EntryData ed = CreateNewTask();
                     DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                     ed.SetAsTask(dueDate);

                     ConsoleWriter.Print("Converted to task and date set to today!");
                 }),

                new Utils.ActionParams(showAllCommands, "t", "t. Convert to Task", delegate (string fullmessage)
                {
                    int id = Utils.GetUserInputInt("Entry ID:", parentID);
                    if (IsEntryValidOrPrintError(id))
                    {
                        DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                        application.OutlineManager.GetEntry(id).SetAsTask(dueDate);
                    }
                }),
                    
                new Utils.ActionParams(showAllCommands, "e", "e. Edit", delegate (string fullmessage) {
                    int id = Utils.GetUserInputInt("Entry ID:", parentID);
                    if (IsEntryValidOrPrintError(id)) {
                        EntryData ed = application.OutlineManager.GetEntry(id);

                        if (ed.IsTask && !ed.IsTaskClosed) {
                            ed.TaskDueDate = Utils.GetDateFromUser("Due Date", ed.TaskDueDate);
                        }
                        ed.title = Utils.GetUserInputString("Enter Title:", ed.title);
                    }
                }),

                new Utils.ActionParams(showAllCommands, "-t", "-t. Convert to Note", delegate (string fullmessage)
                {
                    int id = Utils.GetUserInputInt("Entry ID:", parentID);
                    if (IsEntryValidOrPrintError(id))
                    {
                        //DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                        application.OutlineManager.GetEntry(id).ConvertToNotes();
                    }
                }),


                new Utils.ActionParams(showAllCommands, "atl", "atl. Create a Task and link to this", delegate (string fullmessage)
                {
                    if (IsEntryValidOrPrintError(parentID))
                    {
                        EntryData ed = CreateNewTask();
                        if (ed != null)
                        {
                            DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                            ed.SetAsTask(dueDate);
                            EntryData ed2 = application.OutlineManager.GetEntry(parentID);
                            ed.AddLink(ed2.id);
                            ed2.AddLink(ed.id);
                        }
                    }
                }),

                new Utils.ActionParams(showAllCommands, "clone", "clone. Clone", delegate (string fullmessage)
                {
                    ConsoleWriter.Print("Children will not be cloned. links will be maintained!");
                    bool confirmation = true; // Utils.GetConfirmationFromUser("Do you want to proceed", false);

                    if (confirmation)
                    {
                        string commaSeperatedID = Utils.GetUserInputString("Entry ID (comma seperated):", parentID.ToString());
                        string[] IDs = commaSeperatedID.Split(',');
                        foreach (string idStr in IDs)
                        {
                            int id = -1;

                            if (int.TryParse(idStr, out id) && IsEntryValidOrPrintError(id))
                            {
                                EntryData ed = application.OutlineManager.GetEntry(id);
                                CloneTask(ed);
                                
                            }
                            else
                                ConsoleWriter.PrintInRed("Action not performed");
                        }
                    }
                }),
                new Utils.ActionParams(showAllCommands, "clonegroup", "clonegroup. Clone a group", delegate (string fullmessage)
                {
                    String groupName = Utils.SelectFrom("Groups", "Select group", "habits", "habits", "dailys" );

                    bool confirmation = Utils.GetConfirmationFromUser("Do you want to proceed", false);

                    if (confirmation) {
                        if (groupName == "habits")
                            CloneHabits();
                        else if (groupName == "dailys")
                            CloneTodays();
                        else
                            Utils.Assert(false);
                    }
                }),

                new Utils.ActionParams(showAllCommands, "p", "p. Change parent", delegate (string fullmessage)
                {
                    int id = Utils.GetUserInputInt("Entry ID:", parentID);
                    if (IsEntryValidOrPrintError(id))
                    {
                        EntryData ed = application.OutlineManager.GetEntry(id);
                        int parentID = Utils.GetUserInputInt("Parent ID:");

                        if (IsEntryValidOrPrintError(parentID))
                        {
                            ed.parentID = parentID;
                            ConsoleWriter.Print("Parent ID Changed");
                        }
                    }
                }),

                new Utils.ActionParams(showAllCommands, "pomostart", "pomostart. ", delegate (string fullmessage)
                {
                    String timerType = Utils.SelectFrom("Timer type", "Select timer", "MID", "SMALL", "MID", "BIG", "HUGE");
                    ePomoTimer timer;
                    if (Enum.TryParse( timerType, out timer)) {
                        application.PomoManager.StartNewPomoTimer(timer);
                    }
                    else
                        ConsoleWriter.PrintInRed("Failed to create!");
                }),

                new Utils.ActionParams(showAllCommands, "pomolink", "pomolink. ", delegate (string fullmessage)
                {
                    int id = Utils.GetUserInputInt("Entry ID:", parentID);
                    if (IsEntryValidOrPrintError(id) )
                    {
                        if (!pomoLinks.Contains(id))
                            pomoLinks.Add(id);
                    }
                }),

                new Utils.ActionParams(showAllCommands, "pomoend", "pomoend. ", delegate (string fullmessage)
                {
                    if ( !application.PomoManager.IsActiveTimerEnded() )
                    {
                        ConsoleWriter.PrintInRed("Pomo timer hasnt ended!");
                        return;
                    }
                    if ( pomoLinks.Count == 0 )
                    {
                        ConsoleWriter.Print("We have no pomos linked");
                    }
                    if (pomoLinks.Count > 0 )
                    {
                        ConsoleWriter.PrintWithOutLineBreak("We have these tasks linked:");
                        foreach (int id in pomoLinks)
                            ConsoleWriter.PrintWithOutLineBreak(id + "\t");
                        ConsoleWriter.PrintNewLine();
                    }

                    bool confirmation = Utils.GetConfirmationFromUser("Is that it ?");

                    if (confirmation)
                    {
                        application.PomoManager.EndTimer(pomoLinks);
                        pomoLinks.Clear();
                    }
                }),

                 new Utils.ActionParams(showAllCommands, "pomoprint", "pomoprint. ", delegate (string fullmessage)
                 {
                     ConsoleWriter.PushColor(ConsoleColor.Yellow);
                     ConsoleWriter.Print( "Pomos completed today:" + application.PomoManager.PomosCountToday() );
                     ConsoleWriter.PopColor();

                     if (pomoLinks.Count == 0)
                     {
                         ConsoleWriter.Print("We have no pomos linked");
                     }
                     if (pomoLinks.Count > 0)
                     {
                         ConsoleWriter.PrintWithOutLineBreak("We have these tasks linked:");
                         foreach (int id in pomoLinks)
                             ConsoleWriter.PrintWithOutLineBreak(id + "\t");
                         ConsoleWriter.PrintNewLine();
                     }

                 }),

                new Utils.ActionParams(showAllCommands, "h", "h. hide completed", delegate (string fullmessage)
                {
                    hideCompleted = !hideCompleted;

                    if (hideCompleted)
                        ConsoleWriter.Print("Hiding completed!");
                    else
                        ConsoleWriter.Print("Showing completed");
                }),

                new Utils.ActionParams(showAllCommands, "sort", "sort. by due date", delegate (string fullmessage)
                {
                    sortByDueDate = !sortByDueDate;

                    if (sortByDueDate)
                        ConsoleWriter.Print("Sorting!");
                    else
                        ConsoleWriter.Print("Not sorting!");
                }),
                new Utils.ActionParams(true, "misc", "misc. Show all commands", delegate (string fullmessage)
                {
                    showAllCommands = !showAllCommands;

                    if (showAllCommands)
                        ConsoleWriter.Print("Showing all commands!");
                    else
                        ConsoleWriter.Print("Hiding commands!");
                }),


                new Utils.ActionParams(true, "s", "s. save", delegate (string fullmessage)
                {
                    application.OutlineManager.Save();
                    application.PomoManager.Save();
                }),

                new Utils.ActionParams(true, "x", "x. exit", delegate (string fullmessage)
                {
                    ConsoleWriter.Print("Exiting");
                    if ( application.FSM.StateCount > 1 )
                        Exit();
                })

            );
        }

        private bool IsEntryValidOrPrintError( int id )
        {
            if (application.OutlineManager.IsEntryAvailableWithID(id))
            {
                return true;
            }
            else
                ConsoleWriter.PrintInRed("Parent not found");
            return false;
        }
        private EntryData CreateNewTask ()
        {
            EntryData ed = new EntryData();
            ed.id = application.OutlineManager.GetNewID();
            ed.title = Utils.GetUserInputString("Enter title:");
            ed.parentID = Utils.GetUserInputInt("Enter parent:", parentID);
            ed.CreatedDate = Utils.Now;
            if (application.OutlineManager.IsEntryAvailableWithID(ed.parentID))
            {
                application.OutlineManager.AddEntry(ed);
                return ed;
            }
            else
            { 
                ConsoleWriter.PrintInRed("Parent not found");
                return null;
            }
        }
        private void RemoveTask ( EntryData entryData )
        {
            if (entryData.links.Count > 0)
            {
                List<int> links = new List<int>(entryData.links);
                foreach (int linkID in links)
                {
                    EntryData linkEntryData = application.OutlineManager.GetEntry(linkID);
                    linkEntryData.UnLink(entryData.id);
                    entryData.UnLink(linkEntryData.id);
                }
            }

            List<EntryData> entries = application.OutlineManager.outlineData.entries.FindAll(X => (X != null && X.parentID == entryData.id));
            if (entries.Count > 0)
            {
                foreach (EntryData child in entries)
                {
                    RemoveTask(child);
                    application.OutlineManager.RemoveEntry(child);
                }
            }
            application.OutlineManager.RemoveEntry(entryData);
        }
        private void CloneTask(EntryData entryData)
        {
            EntryData clonesTask = new EntryData();
            clonesTask.id = application.OutlineManager.GetNewID();

            clonesTask.title = entryData.title;
            clonesTask.parentID = entryData.parentID;
            clonesTask.CreatedDate = Utils.Now;
            if (entryData.IsTask)
            {
                DateTime dueDate = Utils.GetDateFromUser("Due Date", Utils.Now.AddDays(14));
                clonesTask.SetAsTask(dueDate);
            }

            application.OutlineManager.AddEntry(clonesTask);

            if (entryData.links.Count > 0)
            {
                foreach (int linkID in entryData.links)
                {
                    EntryData linkEntryData = application.OutlineManager.GetEntry(linkID);
                    linkEntryData.AddLink(clonesTask.id);
                    clonesTask.AddLink(linkEntryData.id);
                }
            }
        }

        private void CloneHabits ()
        {
            int[] taskIDsToClone = new int[] { 86, 88, 89, 90, 91,    92, 95, 96, 192, 385, 336 };

            foreach( int habitTaskID in taskIDsToClone )
            {
                EntryData ed = application.OutlineManager.GetEntry(habitTaskID);
                CloneTask(ed);
            }
        }
        private void CloneTodays() {
            int[] taskIDsToClone = new int[] { 276, 277, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288, 289, 290, 292, 293 };

            foreach (int habitTaskID in taskIDsToClone) {
                EntryData ed = application.OutlineManager.GetEntry(habitTaskID);
                CloneTask(ed);
            }
        }

        private int parentID = -1;
        private JApplication application;
        private static bool hideCompleted = true;
        private static bool sortByDueDate = true;
        private static bool showAllCommands = false;
        private static List<int> pomoLinks = new List<int>();

    }
}
