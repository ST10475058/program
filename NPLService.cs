using System.Text.RegularExpressions;

namespace CybersecurityChatbotPOE.Services
{
    public class NLPService
    {
        public enum Intent { AddTask, ShowTasks, CompleteTask, DeleteTask, StartQuiz, ShowLog, Help, Greeting, CyberQuestion, Unknown }

        public Intent ParseInput(string input)
        {
            string lower = input.ToLower();
            if (lower.Contains("add task") || lower.Contains("new task")) return Intent.AddTask;
            if (lower.Contains("show task") || lower.Contains("my task")) return Intent.ShowTasks;
            if (Regex.IsMatch(lower, @"complete\s+task\s+\d+")) return Intent.CompleteTask;
            if (Regex.IsMatch(lower, @"delete\s+task\s+\d+")) return Intent.DeleteTask;
            if (lower.Contains("quiz") || lower.Contains("test me")) return Intent.StartQuiz;
            if (lower.Contains("activity log") || lower.Contains("what have you done")) return Intent.ShowLog;
            if (lower.Contains("help")) return Intent.Help;
            if (lower.Contains("hello") || lower.Contains("hi")) return Intent.Greeting;
            if (lower.Contains("password") || lower.Contains("phish") || lower.Contains("2fa") || lower.Contains("sim")) return Intent.CyberQuestion;
            return Intent.Unknown;
        }
    }
}