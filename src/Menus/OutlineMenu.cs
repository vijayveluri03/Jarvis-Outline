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
            this.contextOutlineMenu.parentIDForNewTasks = (outlineMenuContext as Context).entryID;

            // Making sure the task exists. 
            Utils.Assert(application.OutlineManager.IsEntryAvailableWithID(this.contextOutlineMenu.parentIDForNewTasks));

            List<Utils.ActionParams> actionParams = new List<Utils.ActionParams>();

            actionParams.Add( application.SharedLogic.CreateActionParamsForNewEntry(this.contextOutlineMenu).SetVisible(true) );
            actionParams.Add(
                new Utils.ActionParams( "j", "j. Jump to", delegate (Utils.aActionParamsContext context) {
                    int id = Utils.GetUserInputInt("Entry ID:");
                    if (SharedLogic.IsEntryValidOrPrintError(id)) {
                        application.FSM.PushInNextFrame(new OutlineMenu(), OutlineMenu.GetContext(application, id));
                    }
            }).SetVisible(true) );
            actionParams.Add(
                new Utils.ActionParams("m", "m. Marked Tasks", delegate (Utils.aActionParamsContext context) {
                    application.FSM.PushInNextFrame(new MarkedTasksMenu(), MarkedTasksMenu.GetContext(application));
                }).SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsForTaskCompletion().SetVisible(true));
            actionParams.Add(application.SharedLogic.CreateActionParamsToDiscardTask().SetVisible(false));

            actionParams.Add(application.SharedLogic.CreateActionParamsToRemoveEntry().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToRefresh().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToLinkTwoEntries().SetVisible(false));

            actionParams.Add(application.SharedLogic.CreateActionParamsToCreateANewTask(this.contextOutlineMenu).SetVisible(true));
            actionParams.Add(application.SharedLogic.CreateActionParamsToConvertToATask().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToEditAnEntry().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToChangeParent().SetVisible(true));

            actionParams.Add(application.SharedLogic.CreateActionParamsToConvertToAnEntry().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToCreateATaskAndLinkItToParent(this.contextOutlineMenu).SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToCloneAnEntry().SetVisible(false));

            actionParams.Add(application.SharedLogic.CreateActionParamsToCloneGroup().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToStartAPomodoroTimer().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToLinkTasksToAPomodoroTimer().SetVisible(false));

            actionParams.Add(application.SharedLogic.CreateActionParamsToEndPomodoroTimer().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToPrintPomodoroStatus().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToSaveAll().SetVisible(true));

            actionParams.Add(application.SharedLogic.CreateActionParamsToMarkATask().SetVisible(false));

            actionParams.Add(
            new Utils.ActionParams("h", "h. show/hide completed", delegate (Utils.aActionParamsContext context) {
                hideCompleted = !hideCompleted;

                if (hideCompleted)
                    ConsoleWriter.Print("Hiding completed!");
                else
                    ConsoleWriter.Print("showing completed!");
            }).SetVisible(true));

            actionParams.Add(
            new Utils.ActionParams( "sort", "sort. by due date", delegate (Utils.aActionParamsContext context) {
                sortByDueDate = !sortByDueDate;

                if (sortByDueDate)
                    ConsoleWriter.Print("Sorting!");
                else
                    ConsoleWriter.Print("Not sorting!");
            }).SetVisible(false));

            actionParams.Add(SharedLogic.CreateActionParamsToShowAllCommands(this.contextOutlineMenu));
            actionParams.Add(
                new Utils.ActionParams("x", "x. back", delegate (Utils.aActionParamsContext context) {
                     Exit();
                }).SetVisible(true)
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

            Utils.Assert(application.OutlineManager.IsEntryAvailableWithID(contextOutlineMenu.parentIDForNewTasks), "Error");
            EntryData entryData = application.OutlineManager.GetEntry(contextOutlineMenu.parentIDForNewTasks);
            UICommon.PrintEntry(entryData);
            
            ConsoleWriter.PopColor();
            
            List<EntryData> entries = application.OutlineManager.outlineData.entries.FindAll(X => (X != null && X.parentID == contextOutlineMenu.parentIDForNewTasks ));
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

        SharedLogic.ActionParamsContext contextOutlineMenu = new SharedLogic.ActionParamsContext();
        private JApplication application;
        private static bool hideCompleted = true;
        private static bool sortByDueDate = true;
        
        

        private Utils.ActionParams[] actionParams = null;

    }
}
