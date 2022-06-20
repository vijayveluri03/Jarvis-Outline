//#define DEBUG_TROUBLESHOOT

using System;
using System.Collections.Generic;
using System.IO;

namespace Jarvis
{
    public class JApplication
    {
        public static JApplication Instance { get { return instance; } }
        private static JApplication instance = null;

        // Getters

        public JDesignData DesignData { get { return designData; } }
        public JUserData UserData { get { return userData; } }
        public TaskManager taskManager { get; private set; }
        public HabitManager habitManager { get; private set; }
        public JournalManager journalManager { get; private set; }
        public TaskTimeManagement logManager { get; private set; }

        public void Initialize()
        {
            instance = this;

            // Set working directory, before we access any files. 
            {
#if DEBUG_TROUBLESHOOT
                Console.WriteLine("Current working directory : " + Environment.CurrentDirectory + " CWD of the program: " + System.Reflection.Assembly.GetExecutingAssembly().Location);
#endif

                // program working directory
                string programWD = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/";
                // Loading Design and user data.
#if DEBUG
                programWD = "c:/Users/vijay/mybin/";
#endif
                JConstants.WORKING_DIRECTORY = programWD;
            }


            designData = JDesignData.Load();

            ConsoleWriter.PushColor(designData.DefaultColorForText);

            userData = JUserData.Load();
            taskManager = new TaskManager();
            habitManager = new HabitManager();
            logManager = new TaskTimeManagement();
            journalManager = new JournalManager();
        }

        public void Save()
        {
            userData.Save();
            taskManager.Save();
            logManager.Save();
            habitManager.Save();
            journalManager.Save();
        }

        // private

        private JUserData userData;
        private JDesignData designData;
    }
}
