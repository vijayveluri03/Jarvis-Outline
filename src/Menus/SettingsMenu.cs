using System;
using System.Collections.Generic;
using System.Linq;

namespace Jarvis {
    public class SettingsMenu : QUtils.FSMBaseState {

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
            Utils.Assert(context != null && context is Context, "Settings Menu initialization error");
            application = (context as Context).application;
        }

        // UI update

        public override void Update() {

            ConsoleWriter.Print("Work in progress!");

            Utils.DoAction("Settings Menu options:", ":", "refresh",

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
