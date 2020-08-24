using System;
using System.Collections.Generic;
using System.Linq;

namespace Jarvis {
    public class MainMenu : QUtils.FSMBaseState {

        // Context

        public class Context {
            public Context(JApplication application) { this.application = application; }
            public JApplication application;
        };

        public static Context GetContext(JApplication application) {
            Context contxt = new Context(application);
            return contxt;
        }

        public override void OnContext(System.Object context) {
            Utils.Assert(context != null && context is Context, "MainMenu initialization error");
            application = (context as Context).application;
        }

        // UI update

        public override void Update() {

            // FTUE - First time user experience 
            if (string.IsNullOrEmpty(application.UserData.userName)) {
                ConsoleWriter.PrintNewLine();
                ConsoleWriter.PrintInColor("Hello there! Welcome to Jarvis Outline. ", ConsoleColor.Yellow);
                ConsoleWriter.Print("This is an outline app, which can help u maintain tasks or notes.");
                application.UserData.userName = Utils.GetUserInputString("Enter your name:");
                application.UserData.activatePomodoro = Utils.GetConfirmationFromUser("Activate Pomodoro? You can change this in settings:");
                application.UserData.activatePomodoroReminder = application.UserData.activatePomodoro && Utils.GetConfirmationFromUser("Activate Pomodoro Reminder/Chimer? You can change this in settings:");
                application.UserData.Save();
                ConsoleWriter.PrintInColor("Awesome. Lets get started", ConsoleColor.Yellow);
            }
            else
                ConsoleWriter.PrintInColor($"Welcome {application.UserData.userName}.", ConsoleColor.Yellow);

            Utils.DoAction("Main Menu options:", ":", "o",

                new Utils.ActionParams( "o", "o. Outline", delegate (Utils.IActionParamsContext context) {
                    application.FSM.PushInNextFrame(new OutlineMenu(), OutlineMenu.GetContext(application, JConstants.ROOT_ENTRY_ID));
                }),

                new Utils.ActionParams( "r", "r. Reports", delegate (Utils.IActionParamsContext context) {
                    application.FSM.PushInNextFrame(new ReportsMenu(), ReportsMenu.GetContext(application));
                }),

                new Utils.ActionParams( "s", "s. Settings", delegate (Utils.IActionParamsContext context) {
                    application.FSM.PushInNextFrame(new SettingsMenu(), SettingsMenu.GetContext(application));
                }),

                new Utils.ActionParams( "s", "s. Save", delegate (Utils.IActionParamsContext context) {
                    application.OutlineManager.Save();
                    application.PomoManager.Save();
                    application.UserData.Save();
                }),

                new Utils.ActionParams( "x", "x. Exit", delegate (Utils.IActionParamsContext context) {
                    Exit();
                })

            );
        }

        public void Exit() {
            application.FSM.Pop();
        }

        // Private

        private JApplication application;
    }
}
