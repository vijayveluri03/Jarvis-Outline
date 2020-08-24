using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;

namespace Jarvis {

    public enum ePomoTimer {
        SMALL = 1,
        MID,
        BIG,
        HUGE
    }
    public class PomoEntry {
        public int id;
        public List<int> links = new List<int>();
        public DateTime startTime;
        public int timeTaken = 25;
    }
    public class PomoEntries {
        public List<PomoEntry> entries = new List<PomoEntry>();
        public DateTime newPomoStartTime = DateTime.MinValue;
        // This is the time which the pomo takes to complete. like 25, 50 etc. ideally it should be always 25, but it can be irritating
        // to constantly be distracted to start a new timer. So for bigger tasks we can go for 50, or 75
        public int minsForNewPomoToExpire = -1;

        public bool StartAnActiveTimer(ePomoTimer timerType ) {
            if (DoWeHaveAnActivePomoTimer()) {
                ConsoleWriter.PrintInRed("We have an active pomo already");
                return false;
            }
            newPomoStartTime = DateTime.Now;
            minsForNewPomoToExpire = -1;
            if (timerType == ePomoTimer.SMALL)
                minsForNewPomoToExpire = JConstants.POMO_SMALL_DURATION_IN_MINS;
            else if (timerType == ePomoTimer.MID)
                minsForNewPomoToExpire = JConstants.POMO_MID_DURATION_IN_MINS;
            else if ( timerType == ePomoTimer.BIG)
                minsForNewPomoToExpire = JConstants.POMO_LARGE_DURATION_IN_MINS;
            else
                minsForNewPomoToExpire = JConstants.POMO_HUGE_DURATION_IN_MINS;
            return true;
        }
        public bool EndTimer(int pomoID, List<int> taskIDs) {
            if (!DoWeHaveAnActivePomoTimer()) {
                ConsoleWriter.PrintInRed("There is no Active Timer to end");
                return false;
            }
            if (!IsActiveTimerEnded()) {
                ConsoleWriter.PrintInRed("There Active timer hasnt ended");
                return false;
            }

            PomoEntry entry = new PomoEntry();
            entry.id = pomoID;
            entry.links = new List<int>(taskIDs);
            entry.startTime = newPomoStartTime;
            entry.timeTaken = GetPomoTotalMinsRequiredToComplete();

            entries.Add(entry);
            ResetActiveTimer();

            return true;
        }

        public void ResetActiveTimer() {
            newPomoStartTime = DateTime.MinValue;
        }
        public bool DoWeHaveAnActivePomoTimer() { return newPomoStartTime != DateTime.MinValue; }
        public bool IsActiveTimerEnded() { if (DoWeHaveAnActivePomoTimer() && (DateTime.Now - newPomoStartTime).TotalMinutes >= GetPomoTotalMinsRequiredToComplete()) return true; return false; }
        public int GetPomoTotalMinsRequiredToComplete () { return (DoWeHaveAnActivePomoTimer() ? minsForNewPomoToExpire : -1); }
    }

    public class PomoManager {

        public PomoEntries PomoEntries { get; private set; }

        public PomoManager(bool enablePomoChime) {

            if ( Utils.CreateFileIfNotExit( JConstants.POMO_FILENAME, JConstants.POMO_TEMPLATE_FILENAME)) {
                ConsoleWriter.Print("Pomodoro data copied from Template. This happens on the first launch.");
            }

            LoadData(JConstants.POMO_FILENAME);

            if (enablePomoChime) {
                Thread workerThread = new Thread(PomoTicker);
                workerThread.IsBackground = true;
                workerThread.Start();
            }

        }

        public bool IsThereAnActivePomoTimer() { return PomoEntries.DoWeHaveAnActivePomoTimer(); }
        public bool IsActiveTimerEnded() { return PomoEntries.IsActiveTimerEnded(); }
        public int GetActiveTime() {
            if (IsThereAnActivePomoTimer()) {
                return (int)(DateTime.Now - PomoEntries.newPomoStartTime).TotalMinutes;
            }
            return -1;
        }

        public bool StartNewPomoTimer( ePomoTimer timerType) {
            return PomoEntries.StartAnActiveTimer( timerType );
        }

        public bool EndTimer(List<int> taskIDs) {
            return PomoEntries.EndTimer(GetNewID(), taskIDs);
        }

        public void ResetTimer() {
            PomoEntries.ResetActiveTimer();
        }

        public int PomosCountToday() {
            int timeTaken = 0;
            foreach (PomoEntry entry in PomoEntries.entries) {
                if (entry.startTime.IsToday())
                    timeTaken += entry.timeTaken;
            }
            return (int)(timeTaken / JConstants.POMO_SMALL_DURATION_IN_MINS);
        }

        public int GetTotalTimeToComplete() { return PomoEntries.GetPomoTotalMinsRequiredToComplete(); }

        public void Save() {
            string serializedData = JsonConvert.SerializeObject(PomoEntries, Formatting.Indented);
            File.WriteAllText(JConstants.POMO_FILENAME, serializedData);
        }

        public int GetNewID() {
            return freeID++;
        }

        public bool IsEntryAvailableWithID(int id) {
            foreach (PomoEntry ed in PomoEntries.entries) {
                if (ed.id == id)
                    return true;
            }
            return false;
        }

        public PomoEntry GetEntry(int id) {
            foreach (PomoEntry ed in PomoEntries.entries) {
                if (ed.id == id)
                    return ed;
            }
            return null;
        }

        private void LoadData(string fileName) {
            
            using (StreamReader r = new StreamReader(fileName)) {
                string json = r.ReadToEnd();
                PomoEntries data = JsonConvert.DeserializeObject<PomoEntries>(json);
                PomoEntries = data;
            }

            foreach (PomoEntry ed in PomoEntries.entries) {
                if (ed.id >= freeID)
                    freeID = ed.id + 1;
            }
        }

        // WIP - Todo create a pomodoro chimer to remind us when pomo ended, or when it has been a while since pomodoro was started. 
        private void PomoTicker() {
            //SoundPlayer typewriter = new SoundPlayer();
            //typewriter.SoundLocation = Environment.CurrentDirectory + "/typewriter.wav";

            while (true) {
                bool wasPomoStarted = IsThereAnActivePomoTimer();

                if (wasPomoStarted) {

                    Thread.Sleep(1000 * 120 );   // chime every 90 seconds when timer ended

                    // todo - load and play a notification file
                    if (IsActiveTimerEnded()) {
                        Console.Beep();
                        Console.Beep();
                    }
                }
                // When its been a while since a new pomo timer was started. 

                else {
                    Console.Beep();
                    Console.Beep();
                    Console.Beep();

                    Thread.Sleep(1000 * 60 * 30);  // Chime every 30 mins
                }
            }
        }

        private int freeID = 1;
    }
}
