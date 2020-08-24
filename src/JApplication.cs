//#define DEBUG_TROUBLESHOOT

using System;
using System.Collections.Generic;
namespace Jarvis {
    public class JApplication {
        public static JApplication Instance { get { return instance; } }
        private static JApplication instance = null;

        // Getters

        public QUtils.FSM FSM { get { return fsm; } }
        public JDesignData DesignData { get { return designData; } }
        public JUserData UserData { get { return userData; } }
        public bool IsExitSignalRaised { get { return exit; } }
        public OutlineManager OutlineManager { get; private set; }
        public PomoManager PomoManager { get; private set; }
        public SharedLogic SharedLogic { get; private set; }

        public void Initialize() {
            instance = this;

    #if DEBUG_TROUBLESHOOT
            Console.WriteLine( "Current working directory : " + Environment.CurrentDirectory);
    #endif

            // Loading Design and user data.

            ConsoleWriter.PushColor(ConsoleColor.White);    // @todo - Load the foreground color from design Data
            fsm = new QUtils.FSM();
            designData = JDesignData.Load();
            SharedLogic = SharedLogic.Load();
            userData = JUserData.Load();
            ConsoleWriter.PopColor();                           // removing temporarily color. @todo -  remove this necessity

            ConsoleWriter.PushColor(designData.defaultColorForText);    // Pushing default foreground color.
                                                                        // Additional colors can be pushed and poped as and when needed.
            Utils.SetMaxColumnsForOptions(designData.ColumnCountForOptions); //// Setting the default spacing

            OutlineManager = new OutlineManager();
            PomoManager = new PomoManager( userData.activatePomodoroReminder );
            SharedLogic.PostInit(OutlineManager, PomoManager);

            fsm.PushInNextFrame(new MainMenu(), MainMenu.GetContext(this));
        }

        public void Update() {
            fsm.Update();
            if (!fsm.IsThereAnyStateToUpdate()) {
                Exit();
                return;
            }
        }
        
        public void Exit() {
            exit = true; 
        }

        // private

        private JUserData userData;
        private JDesignData designData;
        private QUtils.FSM fsm = null;
        private bool exit = false;
    }
}
