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
        public override void OnContext(System.Object outlineMenuContext) {
            Utils.Assert(outlineMenuContext != null && outlineMenuContext is Context, "EntryMenu initialization error");
            application = (outlineMenuContext as Context).application;
            
            List<Utils.ActionParams> actionParams = new List<Utils.ActionParams>();

            
            actionParams.Add(
                new Utils.ActionParams("j", "j. Jump to", delegate (Utils.IActionParamsContext context) {
                    int id = Utils.GetUserInputInt("Entry ID:");
                    if (SharedLogic.IsEntryValidOrPrintError(id)) {
                        application.FSM.PushInNextFrame(new OutlineMenu(), OutlineMenu.GetContext(application, id));
                    }
                }));
            actionParams.Add(
                new Utils.ActionParams("c", "c. Complete", delegate (Utils.IActionParamsContext context) {
                    int id = Utils.GetUserInputInt("Entry ID:");
                    if (SharedLogic.IsEntryValidOrPrintError(id)) {
                        EntryData ed = application.OutlineManager.GetEntry(id);
                        if (ed.IsTask && !ed.IsTaskClosed)
                            SharedLogic.CompleteTask(id);
                        else
                            ConsoleWriter.PrintInRed("This is either not a task, or was already closed. Action could not be performed");
                    }
                }));
            actionParams.Add(
                new Utils.ActionParams("clear", "clear.", delegate (Utils.IActionParamsContext context) {
                    if (Utils.GetConfirmationFromUser("Are you sure:"))
                        application.UserData.markedTaskIDs.Clear();
                }));
            actionParams.Add(
            new Utils.ActionParams("h", "h. show/hide completed", delegate (Utils.IActionParamsContext context) {
                hideCompleted = !hideCompleted;

                if (hideCompleted)
                    ConsoleWriter.Print("Hiding completed!");
                else
                    ConsoleWriter.Print("showing completed!");
            }));

            actionParams.Add(
                new Utils.ActionParams("s", "s. Save", delegate (Utils.IActionParamsContext context) {
                    application.SharedLogic.SaveAll();
                })
            );

            actionParams.Add(
                new Utils.ActionParams("x", "x. exit", delegate (Utils.IActionParamsContext context) {
                    Exit();
                })
                );

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
        
        private Utils.ActionParams[] actionParams = null;

    }
}
