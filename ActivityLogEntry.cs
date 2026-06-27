using System;

namespace CybersecurityChatbotPOE.Models
{
    public class ActivityLogEntry
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string DisplayText => $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Action}: {Details}";
    }
}