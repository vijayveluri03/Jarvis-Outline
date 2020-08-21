using System;
using System.Collections.Generic;

namespace Jarvis {
    public static class UICommon {
        public static void PrintEntry(EntryData entry) {

            List<EntryData> children = JApplication.Instance.OutlineManager.outlineData.entries.FindAll(X => (X != null && X.parentID == entry.id));
            int pendingTasks = 0, totalTasks = 0, totalNotes = 0;
            foreach (EntryData child in children) {
                if (child.IsTaskClosed)
                    totalTasks++;
                else if (child.IsTask) {
                    totalTasks++;
                    pendingTasks++;
                }
                else
                    totalNotes++;
            }

            string childrenText = $"{pendingTasks}/{totalTasks}/{totalNotes}";
            childrenText += " + ";

            pendingTasks = 0; totalTasks = 0; totalNotes = 0;
            foreach (int childID in entry.links) {
                EntryData child = JApplication.Instance.OutlineManager.GetEntry(childID);
                if (child.IsTaskClosed)
                    totalTasks++;
                else if (child.IsTask) {
                    totalTasks++;
                    pendingTasks++;
                }
                else
                    totalNotes++;
            }

            childrenText += $"{pendingTasks}/{totalTasks}/{totalNotes}";

            if (entry.IsTask) {
                string tags = "";
                string endingInfo = "";
                if (entry.IsTaskDiscarded) {
                    tags = "D";
                    endingInfo = entry.TaskClosedDate.ToShortDateString();
                }
                else if (entry.IsTaskComplete) {
                    tags = "C";
                    endingInfo = entry.TaskClosedDate.ToShortDateString();
                }
                else if (entry.IsTask) {
                    tags = "T";
                    endingInfo = (entry.TaskDueDate - Utils.Now).Days.ToString();
                }

                ConsoleWriter.Print($"{entry.id,-10}{"[" + tags + "] " + entry.title,-200}{childrenText,-15}{endingInfo,-10} ");
            }
            else {
                string endingInfo = entry.CreatedDate.ToShortDateString();
                ConsoleWriter.Print($"{entry.id,-10}{entry.title,-200}{childrenText,-15}{endingInfo,-10}");
            }
        }

        public static void PrintEntryWithColor(EntryData entry) {
            bool pushed = false;
            if (entry.IsTaskDiscarded) {
                ConsoleWriter.PushColor(ConsoleColor.DarkGray);
                pushed = true;
            }
            else if (entry.IsTaskComplete) {
                ConsoleWriter.PushColor(ConsoleColor.DarkGreen);
                pushed = true;
            }
            else if (!entry.TaskDueDate.IsThisMinDate() && (entry.TaskDueDate - Utils.Now).TotalDays <= 1) {
                ConsoleWriter.PushColor(ConsoleColor.Red);
                pushed = true;
            }

            PrintEntry(entry);

            if (pushed)
                ConsoleWriter.PopColor();

        }
    }
}
