﻿//#define DEBUG_TROUBLESHOOT

using System;
using System.Collections.Generic;
using System.IO;

namespace Jarvis
{
    public class JModel
    {
        // Getters

        public JDesignData DesignData { get { return designData; } }
        public JUserData UserData { get { return userData; } }
        public TaskManager taskManager { get; private set; }
        public HabitManager habitManager { get; private set; }
        public NotebookManager notebookManager { get; private set; }
        public TaskTimeManagement logManager { get; private set; }
        public PomoManager pomoManager { get; private set; }

        public void InitializeCore()
        {
            designData = JDesignData.Load();
        }
        public void InitializeRest()
        {
            userData = JUserData.Load();
            taskManager = new TaskManager(designData.TaskDefaultStatus, designData.DoesTaskStatusExist);
            habitManager = new HabitManager();
            logManager = new TaskTimeManagement();
            notebookManager = new NotebookManager(designData.NotebookDefaultTag, designData.DoesNotebookTagExist);
            pomoManager = new PomoManager();
        }

        public void Save()
        {
            userData.Save();
            taskManager.Save();
            logManager.Save();
            habitManager.Save();
            notebookManager.Save();
            pomoManager.Save();
        }

        // private

        private JUserData userData;
        private JDesignData designData;
    }
}
