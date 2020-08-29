using System;
using System.Collections.Generic;
using System.Linq;

namespace Jarvis {
    public class FilteredTasksMenu : QUtils.FSMBaseState {

        public enum eFilter {
            MARKED_TASKS,
            DUE_TASKS
        };

        public SharedLogic SharedLogic { get { return application.SharedLogic; } }
        public class Context {
            public Context(JApplication application, eFilter filter) { this.application = application; this.filter = filter; }
            public JApplication application;
            public eFilter filter;
        };

        public static Context GetContext(JApplication application, eFilter filter ) {
            Context contxt = new Context(application, filter);
            return contxt;
        }

        public void Exit() {
            application.FSM.Pop();
        }
        public override void OnContext(System.Object filteredMenuContext) {
            Utils.Assert(filteredMenuContext != null && filteredMenuContext is Context, "EntryMenu initialization error");
            application = (filteredMenuContext as Context).application;
            filterApplied = (filteredMenuContext as Context).filter;

            List<Utils.ActionParams> actionParams = new List<Utils.ActionParams>();

            actionParams.Add(
                new Utils.ActionParams("j", "j. Jump to", delegate (Utils.aActionParamsContext context) {
                    int id = Utils.GetUserInputInt("Entry ID:");
                    if (SharedLogic.IsEntryValidOrPrintError(id)) {
                        application.FSM.PushInNextFrame(new OutlineMenu(), OutlineMenu.GetContext(application, id));
                    }
                }).SetVisible(true));
            actionParams.Add(
                new Utils.ActionParams("m", "m. Marked Tasks", delegate (Utils.aActionParamsContext context) {
                    application.FSM.PushInNextFrame(new FilteredTasksMenu(), FilteredTasksMenu.GetContext(application, eFilter.MARKED_TASKS));
                }).SetVisible(false));
            actionParams.Add(
                new Utils.ActionParams("d", "d. Due Tasks", delegate (Utils.aActionParamsContext context) {
                    application.FSM.PushInNextFrame(new FilteredTasksMenu(), FilteredTasksMenu.GetContext(application, eFilter.DUE_TASKS));
                }).SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsForTaskCompletion().SetVisible(true));
            actionParams.Add(application.SharedLogic.CreateActionParamsToDiscardTask().SetVisible(false));

            actionParams.Add(application.SharedLogic.CreateActionParamsToRemoveEntry().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToRefresh().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToLinkTwoEntries().SetVisible(false));

            actionParams.Add(application.SharedLogic.CreateActionParamsToConvertToATask().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToEditAnEntry().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToChangeParent().SetVisible(true));

            actionParams.Add(application.SharedLogic.CreateActionParamsToConvertToAnEntry().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToCloneAnEntry().SetVisible(false));

            actionParams.Add(application.SharedLogic.CreateActionParamsToCloneGroup().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToStartAPomodoroTimer().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToLinkTasksToAPomodoroTimer().SetVisible(false));

            actionParams.Add(application.SharedLogic.CreateActionParamsToEndPomodoroTimer().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToPrintPomodoroStatus().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToSaveAll().SetVisible(true));

            actionParams.Add(application.SharedLogic.CreateActionParamsToClearURL().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToOpenURL().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToSetURL().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToPrintURL().SetVisible(false));

            actionParams.Add(application.SharedLogic.CreateActionParamsToMarkATask().SetVisible(false));
            actionParams.Add(application.SharedLogic.CreateActionParamsToPinAnEntry().SetVisible(false));

            actionParams.Add(
            new Utils.ActionParams("h", "h. show/hide completed", delegate (Utils.aActionParamsContext context) {
                hideCompleted = !hideCompleted;
                if (hideCompleted)
                    ConsoleWriter.Print("Hiding completed!");
                else
                    ConsoleWriter.Print("showing completed!");
            }).SetVisible(true));
            actionParams.Add(
            new Utils.ActionParams("switch", "switch. show/hide Duedates", delegate (Utils.aActionParamsContext context) {
                filterApplied = ((filterApplied == eFilter.DUE_TASKS) ? eFilter.MARKED_TASKS : eFilter.DUE_TASKS);
                if (filterApplied == eFilter.DUE_TASKS)
                    ConsoleWriter.Print("Showing tasks with due dates");
                else
                    ConsoleWriter.Print("Showing marked tasks!");
            }).SetVisible(true));


            actionParams.Add(SharedLogic.CreateActionParamsToShowAllCommands(context));
            actionParams.Add(
                new Utils.ActionParams("x", "x. back", delegate (Utils.aActionParamsContext context) {
                    Exit();
                }).SetVisible(true)
                );
            actionParams.Add(
                new Utils.ActionParams("clearmarked", "clearmarked. marked tasks", delegate (Utils.aActionParamsContext context) {
                    if (Utils.GetConfirmationFromUser("Are you sure:"))
                        application.UserData.markedTaskIDs.Clear();
                }).SetVisible(true) );
            actionParams.Add(
                new Utils.ActionParams("clearpinned", "clearpinned. pinned tasks", delegate (Utils.aActionParamsContext context) {
                    if (Utils.GetConfirmationFromUser("Are you sure:"))
                        application.UserData.pinnedTaskIDs.Clear();
                }).SetVisible(true));

            this.actionParams = actionParams.ToArray();
        }

        public override void Update() {
            if (application.PomoManager.IsThereAnActivePomoTimer()) {
                ConsoleWriter.PushColor(ConsoleColor.Yellow);

                if (application.PomoManager.IsActiveTimerEnded())
                    ConsoleWriter.Print("TIMER ENDED");
                else
                    ConsoleWriter.Print("TIMER RUNNING - " + application.PomoManager.GetActiveTime() + " --> " + application.PomoManager.GetTotalTimeToComplete()); ;

                ConsoleWriter.PopColor();
            }
            ConsoleWriter.PushColor(ConsoleColor.DarkBlue);

            
            ConsoleWriter.PopColor();

            List<EntryData> markedorPendingEntries = new List<EntryData>();
            List<EntryData> pinnedEntries = new List<EntryData>();

            foreach (int id in application.UserData.pinnedTaskIDs) {
                if (application.OutlineManager.IsEntryAvailableWithID(id)) {
                    EntryData ed = application.OutlineManager.GetEntry(id);
                    pinnedEntries.Add(ed);
                }
            }

            if (filterApplied == eFilter.MARKED_TASKS) {
                foreach (int id in application.UserData.markedTaskIDs) {
                    if (application.OutlineManager.IsEntryAvailableWithID(id)) {
                        EntryData ed = application.OutlineManager.GetEntry(id);
                            markedorPendingEntries.Add(ed);
                    }
                }
            }
            else {
                markedorPendingEntries = application.OutlineManager.outlineData.entries.FindAll(X => (X != null && X.IsTask && !X.IsTaskClosed && X.DaysRemainingFromDueDate <= 0 ));
            }


            
            if (markedorPendingEntries.Count > 0) {
                int lineCount = 0;
                foreach (EntryData entry in markedorPendingEntries) {
                    if (!hideCompleted || (hideCompleted && !entry.IsTaskClosed)) {
                        UICommon.PrintEntryWithColor(entry);
                        lineCount++;
                        if (lineCount == 3) {
                            ConsoleWriter.PrintNewLine();
                            lineCount = 0;
                        }
                    }
                }
            }
            else {
                ConsoleWriter.Print("<Empty>");
            }

            if (pinnedEntries.Count > 0) {
                ConsoleWriter.PrintNewLine();
                ConsoleWriter.PrintInColor("PINNED NOTES", ConsoleColor.DarkYellow);
                int lineCount = 0;
                foreach (EntryData entry in pinnedEntries) {
                    UICommon.PrintEntryWithColor(entry);
                    lineCount++;
                    if (lineCount == 3) {
                        ConsoleWriter.PrintNewLine();
                        lineCount = 0;
                    }
                }
            }

            ConsoleWriter.PrintNewLine();

            Utils.DoAction("Outline Menu options:", ":", "refresh",
                actionParams
            );
        }

        private JApplication application;
        private static bool hideCompleted = true;
        private static eFilter filterApplied = eFilter.MARKED_TASKS;
        private static Utils.aActionParamsContext context = new Utils.aActionParamsContext();
        
        private Utils.ActionParams[] actionParams = null;

    }
}
