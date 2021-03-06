﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Jarvis {
    public class ReportsMenu : QUtils.FSMBaseState {

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
            Utils.Assert(context != null && context is Context, "Reports Menu initialization error");
            application = (context as Context).application;
        }

        // UI update

        public override void Update() {

            ConsoleWriter.PrintInRed("Work in progress!");

            Utils.DoAction("Reports Menu options:", ":", "x",

                new Utils.ActionParams( "x", "x. Exit", delegate (Utils.aActionParamsContext context) {
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
