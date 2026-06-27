using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CybersecurityChatbotPOE.Models;

namespace CybersecurityChatbotPOE.Services
{
    public class TaskService
    {
        private List<TaskItem> tasks = new List<TaskItem>();
        private int nextId = 1;
        private DatabaseService db;

        public TaskService(DatabaseService dbService)
        {
            db = dbService;
            // Start with empty list - no database loading
            tasks = new List<TaskItem>();
        }

        public async Task<(bool success, string message)> AddTask(string title, string desc, int days = 0)
        {
            if (string.IsNullOrWhiteSpace(title))
                return (false, "❌ Title required");

            var task = new TaskItem
            {
                Id = nextId++,
                Title = title,
                Description = desc ?? "No description"
            };

            if (days > 0)
                task.ReminderDate = System.DateTime.Now.AddDays(days);

            tasks.Add(task);
            await db.AddLog("Task Added", title);

            return (true, $"✅ Task '{title}' added! (Task #{task.Id})");
        }

        public async Task<(bool success, string message)> CompleteTask(int id)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return (false, "❌ Task not found");

            if (task.IsCompleted)
                return (false, $"ℹ️ Task '{task.Title}' is already completed");

            task.IsCompleted = true;
            await db.AddLog("Task Completed", task.Title);

            return (true, $"🎉 Completed '{task.Title}'!");
        }

        public async Task<(bool success, string message)> DeleteTask(int id)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return (false, "❌ Task not found");

            tasks.Remove(task);
            await db.AddLog("Task Deleted", task.Title);

            return (true, $"🗑️ Deleted '{task.Title}'");
        }

        public async Task<(bool success, string message)> ResetCompletedTasks()
        {
            if (!tasks.Any())
                return (false, "📋 No tasks to reset.");

            var completedTasks = tasks.Where(t => t.IsCompleted).ToList();
            if (!completedTasks.Any())
                return (false, "📋 No completed tasks to reset.");

            int deletedCount = 0;
            foreach (var task in completedTasks)
            {
                tasks.Remove(task);
                deletedCount++;
            }

            await db.AddLog("Tasks Reset", $"Deleted {deletedCount} completed tasks");
            return (true, $"🗑️ Reset {deletedCount} completed task(s)!");
        }

        public async Task<(bool success, string message)> ResetAllTasks()
        {
            if (!tasks.Any())
                return (false, "📋 No tasks to reset.");

            int count = tasks.Count;
            tasks.Clear();
            nextId = 1; // Reset the ID counter

            await db.AddLog("All Tasks Reset", $"Deleted all {count} tasks");
            return (true, $"🗑️ Reset ALL {count} task(s)!");
        }

        public string GetTasksDisplay()
        {
            if (!tasks.Any())
                return "📋 No tasks. Try 'add task'";

            string result = "📋 MY TASKS\n━━━━━━━━━━━━━━━━━━━━\n";
            var pending = tasks.Where(t => !t.IsCompleted).ToList();
            var completed = tasks.Where(t => t.IsCompleted).ToList();

            if (pending.Any())
            {
                result += "\n📌 PENDING TASKS:\n";
                foreach (var t in pending)
                    result += $"   {t.Id}. {t.Title}\n";
            }

            if (completed.Any())
            {
                result += "\n✅ COMPLETED TASKS:\n";
                foreach (var t in completed)
                    result += $"   {t.Id}. {t.Title} ✓\n";
            }

            if (!pending.Any() && !completed.Any())
            {
                result += "   All done! 🎉\n";
            }

            result += "\n━━━━━━━━━━━━━━━━━━━━\n";
            result += "Type 'reset completed' to delete done tasks\n";
            result += "Type 'reset all' to delete ALL tasks";

            return result;
        }

        // Helper method to check if tasks exist
        public bool HasTasks() => tasks.Any();

        // Helper method to get task count
        public int GetTaskCount() => tasks.Count;

        // Helper method to get pending task count
        public int GetPendingCount() => tasks.Count(t => !t.IsCompleted);

        // Helper method to get completed task count
        public int GetCompletedCount() => tasks.Count(t => t.IsCompleted);
    }
}