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

            // First time launch
            if (string.IsNullOrEmpty(application.UserData.userName)) {
                ConsoleWriter.PrintInColor("Hello there! Welcome to Jarvis Outline. ", ConsoleColor.Yellow);
                ConsoleWriter.Print("This is an outline app, which can help u maintain tasks or notes.");
                application.UserData.userName = Utils.GetUserInputString("Enter your name:");
                application.UserData.activatePomodoro = Utils.GetConfirmationFromUser("Activate Pomodoro? You can change this in settings:");
                application.UserData.activatePomodoroReminder = Utils.GetConfirmationFromUser("Activate Pomodoro Reminder? You can change this in settings:");
                application.UserData.Save();
                ConsoleWriter.PrintInColor("Awesome. Lets get started", ConsoleColor.Yellow);
            }
            else
                ConsoleWriter.PrintInColor($"Welcome {application.UserData.userName}.", ConsoleColor.Yellow);

            Utils.DoAction("Main Menu options:", ":", "refresh",

                new Utils.ActionParams(true, "o", "o. Outline", delegate (string fullmessage) {
                    application.FSM.PushInNextFrame(new OutlineMenu(), OutlineMenu.GetContext(application, JConstants.ROOT_ENTRY_ID));
                }),

                new Utils.ActionParams(true, "r", "r. Reports", delegate (string fullmessage) {
                    application.FSM.PushInNextFrame(new ReportsMenu(), ReportsMenu.GetContext(application));
                }),

                new Utils.ActionParams(true, "s", "s. Settings", delegate (string fullmessage) {
                    application.FSM.PushInNextFrame(new SettingsMenu(), SettingsMenu.GetContext(application));
                }),

                new Utils.ActionParams(true, "s", "s. save", delegate (string fullmessage) {
                    application.OutlineManager.Save();
                    application.PomoManager.Save();
                    application.UserData.Save();
                }),

                new Utils.ActionParams(true, "x", "x. exit", delegate (string fullmessage) {
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
