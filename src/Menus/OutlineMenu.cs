using System;
using System.Collections.Generic;
using System.Linq;

namespace Jarvis
{
    public class OutlineMenu : QUtils.FSMBaseState
    {
        public SharedLogic SharedLogic {  get { return application.SharedLogic; } }
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
        public override void OnContext(System.Object outlineMenuContext) 
        {
            Utils.Assert(outlineMenuContext != null && outlineMenuContext is Context, "EntryMenu initialization error");
            application = (outlineMenuContext as Context).application;
            this.contextOutlineMenu.parentID = (outlineMenuContext as Context).entryID;

            // Making sure the task exists. 
            Utils.Assert(application.OutlineManager.IsEntryAvailableWithID(this.contextOutlineMenu.parentID));

            List<Utils.ActionParams> actionParams = new List<Utils.ActionParams>();

            actionParams.Add( application.SharedLogic.CreateActionParamsForNewEntry(this.contextOutlineMenu) );
            actionParams.Add(
                new Utils.ActionParams( "j", "j. Jump to", delegate (Utils.IActionParamsContext context) {
                    int id = Utils.GetUserInputInt("Entry ID:");
                    if (SharedLogic.IsEntryValidOrPrintError(id)) {
                        application.FSM.PushInNextFrame(new OutlineMenu(), OutlineMenu.GetContext(application, id));
                    }
            }) );
            actionParams.Add(application.SharedLogic.CreateActionParamsForTaskCompletion(this.contextOutlineMenu));
            actionParams.Add(application.SharedLogic.CreateActionParamsToDiscardTask(this.contextOutlineMenu));

            actionParams.Add(application.SharedLogic.CreateActionParamsToRemoveEntry(this.contextOutlineMenu));
            actionParams.Add(application.SharedLogic.CreateActionParamsToRefresh(this.contextOutlineMenu));
            actionParams.Add(application.SharedLogic.CreateActionParamsToLinkTwoEntries(this.contextOutlineMenu));

            actionParams.Add(application.SharedLogic.CreateActionParamsToCreateANewTask(this.contextOutlineMenu));
            actionParams.Add(application.SharedLogic.CreateActionParamsToConvertToATask(this.contextOutlineMenu));
            actionParams.Add(application.SharedLogic.CreateActionParamsToEditAnEntry(this.contextOutlineMenu));

            actionParams.Add(application.SharedLogic.CreateActionParamsToConvertToAnEntry(this.contextOutlineMenu));
            actionParams.Add(application.SharedLogic.CreateActionParamsToCreateATaskAndLinkItToParent(this.contextOutlineMenu));
            actionParams.Add(application.SharedLogic.CreateActionParamsToCloneAnEntry(this.contextOutlineMenu));

            actionParams.Add(application.SharedLogic.CreateActionParamsToChangeParent(this.contextOutlineMenu));
            actionParams.Add(application.SharedLogic.CreateActionParamsToStartAPomodoroTimer(this.contextOutlineMenu));
            actionParams.Add(application.SharedLogic.CreateActionParamsToLinkTasksToAPomodoroTimer(this.contextOutlineMenu));

            actionParams.Add(application.SharedLogic.CreateActionParamsToEndPomodoroTimer(this.contextOutlineMenu));
            actionParams.Add(application.SharedLogic.CreateActionParamsToPrintPomodoroStatus(this.contextOutlineMenu));
            actionParams.Add(application.SharedLogic.CreateActionParamsToSaveAll(this.contextOutlineMenu));

            actionParams.Add(application.SharedLogic.CreateActionParamsToStartAPomodoroTimer(this.contextOutlineMenu));
            actionParams.Add(application.SharedLogic.CreateActionParamsToStartAPomodoroTimer(this.contextOutlineMenu));
            actionParams.Add(application.SharedLogic.CreateActionParamsToStartAPomodoroTimer(this.contextOutlineMenu));



            actionParams.Add(
            new Utils.ActionParams( "sort", "sort. by due date", delegate (Utils.IActionParamsContext context) {
                sortByDueDate = !sortByDueDate;

                if (sortByDueDate)
                    ConsoleWriter.Print("Sorting!");
                else
                    ConsoleWriter.Print("Not sorting!");
            }));

            actionParams.Add(
                new Utils.ActionParams("misc", "misc. Show all commands", delegate (Utils.IActionParamsContext context) {
                    SharedLogic.OutlineMenuActionParamsContext.showThisParam = !SharedLogic.OutlineMenuActionParamsContext.showThisParam;

                    if (SharedLogic.OutlineMenuActionParamsContext.showThisParam)
                        ConsoleWriter.Print("Showing all commands!");
                    else
                        ConsoleWriter.Print("Hiding commands!");
                }));
            actionParams.Add(
                new Utils.ActionParams("x", "x. exit", delegate (Utils.IActionParamsContext context) {
                    ConsoleWriter.Print("Exiting");
                    if (application.FSM.StateCount > 1)
                        Exit();
                })
                );

            this.actionParams = actionParams.ToArray();
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

            Utils.Assert(application.OutlineManager.IsEntryAvailableWithID(contextOutlineMenu.parentID), "Error");
            EntryData entryData = application.OutlineManager.GetEntry(contextOutlineMenu.parentID);
            UICommon.PrintEntry(entryData);
            
            ConsoleWriter.PopColor();
            
            List<EntryData> entries = application.OutlineManager.outlineData.entries.FindAll(X => (X != null && X.parentID == contextOutlineMenu.parentID ));
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
                actionParams
            );
        }

        SharedLogic.OutlineMenuActionParamsContext contextOutlineMenu = new SharedLogic.OutlineMenuActionParamsContext();
        private JApplication application;
        private static bool hideCompleted = true;
        private static bool sortByDueDate = true;
        
        

        private Utils.ActionParams[] actionParams = null;

    }
}
