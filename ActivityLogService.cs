using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CybersecurityChatbotPOE.Models;

namespace CybersecurityChatbotPOE.Services
{
    public class ActivityLogService
    {
        private DatabaseService db;

        public ActivityLogService(DatabaseService dbService)
        {
            db = dbService;
        }

        public async Task LogAction(string action, string details)
        {
            await db.AddLog(action, details);
        }

        public async Task<List<ActivityLogEntry>> GetLogEntries(int limit = 10)
        {
            return await db.GetLogs(limit);
        }

        public string GetLogDisplay(List<ActivityLogEntry> entries)
        {
            if (!entries.Any()) return "📋 No activity logged yet.";

            string result = "📊 RECENT ACTIVITY\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";
            for (int i = 0; i < entries.Count; i++)
            {
                result += $"{i + 1}. {entries[i].DisplayText}\n";
            }
            return result;
        }
    }
}