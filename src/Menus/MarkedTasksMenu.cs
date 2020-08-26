using System;
using System.Collections.Generic;
using System.Linq;

namespace Jarvis {
    public class MarkedTasksMenu : QUtils.FSMBaseState {
        public SharedLogic SharedLogic { get { return application.SharedLogic; } }
        public class Context {
            public Context(JApplication application) { this.application = application; }
            public JApplication application;
        };

        public static Context GetContext(JApplication application) {
            Context contxt = new Context(application);
            return contxt;
        }

        public void Exit() {
            application.FSM.Pop();
        }
        public override void OnContext(System.Object markedTaskContext) {
            Utils.Assert(markedTaskContext != null && markedTaskContext is Context, "EntryMenu initialization error");
            application = (markedTaskContext as Context).application;
            
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
                    application.FSM.PushInNextFrame(new MarkedTasksMenu(), MarkedTasksMenu.GetContext(application));
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

            actionParams.Add(application.SharedLogic.CreateActionParamsToMarkATask().SetVisible(false));

            actionParams.Add(
            new Utils.ActionParams("h", "h. show/hide completed", delegate (Utils.aActionParamsContext context) {
                hideCompleted = !hideCompleted;

                if (hideCompleted)
                    ConsoleWriter.Print("Hiding completed!");
                else
                    ConsoleWriter.Print("showing completed!");
            }).SetVisible(true));


            actionParams.Add(SharedLogic.CreateActionParamsToShowAllCommands(context));
            actionParams.Add(
                new Utils.ActionParams("x", "x. back", delegate (Utils.aActionParamsContext context) {
                    Exit();
                }).SetVisible(true)
                );
            actionParams.Add(
                new Utils.ActionParams("clear", "clear.", delegate (Utils.aActionParamsContext context) {
                    if (Utils.GetConfirmationFromUser("Are you sure:"))
                        application.UserData.markedTaskIDs.Clear();
                }).SetVisible(true) );

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

            List<EntryData> entries = new List<EntryData>();

            foreach( int id in application.UserData.markedTaskIDs ) {
                if (application.OutlineManager.IsEntryAvailableWithID(id))
                    entries.Add(application.OutlineManager.GetEntry(id));
            }
            
            if (entries.Count > 0) {
                int lineCount = 0;
                foreach (EntryData entry in entries) {
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

            ConsoleWriter.PrintNewLine();

            Utils.DoAction("Outline Menu options:", ":", "refresh",
                actionParams
            );
        }

        private JApplication application;
        private static bool hideCompleted = true;
        private static Utils.aActionParamsContext context = new Utils.aActionParamsContext();
        
        private Utils.ActionParams[] actionParams = null;

    }
}
