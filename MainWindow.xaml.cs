using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CybersecurityChatbotPOE.Models;
using CybersecurityChatbotPOE.Services;
using CybersecurityChatbotPOE.Views;

namespace CybersecurityChatbotPOE
{
    public partial class MainWindow : Window
    {
        private DatabaseService dbService;
        private TaskService taskService;
        private QuizService quizService;
        private ActivityLogService logService;

        public MainWindow()
        {
            InitializeComponent();
            InitializeServices();
            LoadAsciiArt();
            ShowWelcomeMessage();
        }

        private void InitializeServices()
        {
            dbService = new DatabaseService();
            taskService = new TaskService(dbService);
            quizService = new QuizService(dbService);
            logService = new ActivityLogService(dbService);

            if (dbService.IsConnected())
            {
                DbStatusText.Text = "✅ Database: Connected";
                DbStatusText.Foreground = Brushes.Green;
            }
            else
            {
                string error = dbService.GetConnectionError();
                DbStatusText.Text = $"⚠️ Database: {error}";
                DbStatusText.Foreground = Brushes.Orange;
            }
        }

        private void LoadAsciiArt()
        {
            string art = @"╔═══════════════════════════════════════════════════════════════════╗
║              🔒 CYBER ALLIANCE PROTECTION SYSTEM 🔒              ║
║              PROTECTING SOUTH AFRICAN CITIZENS                   ║
║    ██████╗██╗   ██╗██████╗ ███████╗██████╗ ██████╗  ██████╗      ║
║   ██╔═══╝╚██╗ ██╔╝██╔══██╗██╔═══╝██╔══██╗██╔══██╗██╔═══╝       ║
║   ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝██████╔╝██║            ║
║   ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗██╔═══╝ ██║            ║
║   ╚██████╗   ██║   ██║  ██║███████╗██║  ██║██║     ╚██████╗      ║
║    ╚═════╝   ╚═╝   ╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝╚═╝      ╚═════╝      ║
╚═══════════════════════════════════════════════════════════════════╝";
            AsciiArtDisplay.Text = art;
        }

        private void ShowWelcomeMessage()
        {
            AddBotMessage("Welcome to the Cybersecurity Awareness Chatbot!", "#2ECC71");
            AddBotMessage("📌 NOTE: Tasks are temporary and will be cleared when you close the app.", "#F39C12");
            AddBotMessage("I can help you with tasks, quizzes, and security questions.", "#3498DB");
            AddBotMessage("Try: 'add task', 'show tasks', 'start quiz', or ask about 'password'", "#F39C12");
        }

        private void AddUserMessage(string message)
        {
            var border = new Border
            {
                Margin = new Thickness(10, 5, 50, 5),
                Padding = new Thickness(12, 8, 12, 8),
                CornerRadius = new CornerRadius(15),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#415A77"))
            };
            border.Child = new TextBlock
            {
                Text = $"👤 {message}",
                Foreground = Brushes.White,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };
            ChatPanel.Children.Add(border);
            ScrollToBottom();
        }

        private void AddBotMessage(string message, string colorHex = "#E0E1DD")
        {
            var border = new Border
            {
                Margin = new Thickness(50, 5, 10, 5),
                Padding = new Thickness(12, 8, 12, 8),
                CornerRadius = new CornerRadius(15),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B263B")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71")),
                BorderThickness = new Thickness(1)
            };
            border.Child = new TextBlock
            {
                Text = $"🛡️ {message}",
                Foreground = (Brush)new BrushConverter().ConvertFromString(colorHex),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };
            ChatPanel.Children.Add(border);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToBottom();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await ProcessInput();
        }

        private async void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await ProcessInput();
        }

        private async Task ProcessInput()
        {
            string input = InputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            InputTextBox.Clear();
            AddUserMessage(input);
            TypingIndicator.Text = "🛡️ Typing...";
            await Task.Delay(300);

            string response = "";
            string lower = input.ToLower();

            if (lower.Contains("add task"))
            {
                var dialog = new AddTaskDialog();
                if (dialog.ShowDialog() == true)
                {
                    var result = await taskService.AddTask(dialog.TaskTitle, dialog.TaskDescription);
                    response = result.message;
                }
                else response = "Task cancelled.";
            }
            else if (lower.Contains("show task") || lower.Contains("my task") || lower.Contains("list task"))
            {
                response = taskService.GetTasksDisplay(); // REMOVED await - it returns string directly
            }
            else if (lower.Contains("reset completed") || lower.Contains("reset done"))
            {
                var result = await taskService.ResetCompletedTasks();
                response = result.message;
            }
            else if (lower.Contains("reset all") || lower.Contains("reset everything"))
            {
                var confirm = MessageBox.Show("Are you sure you want to delete ALL tasks?",
                    "Confirm Reset All", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm == MessageBoxResult.Yes)
                {
                    var result = await taskService.ResetAllTasks();
                    response = result.message;
                }
                else
                {
                    response = "Reset cancelled.";
                }
            }
            else if (lower.Contains("complete task"))
            {
                // Extract the number from the command
                int taskNumber = 1;
                foreach (char c in lower)
                {
                    if (char.IsDigit(c))
                    {
                        taskNumber = int.Parse(c.ToString());
                        break;
                    }
                }
                var result = await taskService.CompleteTask(taskNumber);
                response = result.message;
            }
            else if (lower.Contains("complete"))
            {
                response = "Tell me which task number. Example: 'complete task 1'";
            }
            else if (lower.Contains("delete task"))
            {
                int taskNumber = 1;
                foreach (char c in lower)
                {
                    if (char.IsDigit(c))
                    {
                        taskNumber = int.Parse(c.ToString());
                        break;
                    }
                }
                var result = await taskService.DeleteTask(taskNumber);
                response = result.message;
            }
            else if (lower.Contains("delete"))
            {
                response = "Tell me which task number. Example: 'delete task 1'";
            }
            else if (lower.Contains("start quiz") || lower.Contains("take quiz"))
            {
                await logService.LogAction("Quiz", "User started quiz");
                var quizWindow = new QuizWindow(quizService);
                quizWindow.Owner = this;
                quizWindow.ShowDialog();
                response = "Quiz finished! Check your score.";
            }
            else if (lower.Contains("activity log") || lower.Contains("what have you done"))
            {
                var entries = await logService.GetLogEntries();
                response = logService.GetLogDisplay(entries);
            }
            else if (lower.Contains("help"))
            {
                response = "Commands:\n" +
                           "• 'add task' - Add a new task\n" +
                           "• 'show tasks' - View all tasks\n" +
                           "• 'complete task 1' - Complete task #1\n" +
                           "• 'delete task 1' - Delete task #1\n" +
                           "• 'reset completed' - Delete all completed tasks\n" +
                           "• 'reset all' - Delete ALL tasks (confirmation required)\n" +
                           "• 'start quiz' - Take cybersecurity quiz\n" +
                           "• 'activity log' - View activity log";
            }
            else if (lower.Contains("hello") || lower.Contains("hi") || lower.Contains("hey"))
            {
                response = "Hello! How can I help you today?";
            }
            else if (lower.Contains("password"))
            {
                response = "🔐 Use 12+ characters with uppercase, lowercase, numbers, and symbols! Never reuse passwords.";
            }
            else if (lower.Contains("phish") || lower.Contains("phishing"))
            {
                response = "🎣 Never click links in unexpected emails. Check sender addresses carefully!";
            }
            else if (lower.Contains("2fa") || lower.Contains("two factor"))
            {
                response = "🔐 Two-Factor Authentication adds an extra security layer. Use Google Authenticator!";
            }
            else if (lower.Contains("sim swap"))
            {
                response = "📱 Set a SIM PIN with your provider and use app-based 2FA instead of SMS!";
            }
            else
            {
                response = "🤔 Try 'help' to see what I can do! Ask about passwords, phishing, or 2FA.";
            }

            AddBotMessage(response);
            TypingIndicator.Text = "🛡️ Ready";
            InputTextBox.Focus();
        }

        private async void AddTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddTaskDialog();
            if (dialog.ShowDialog() == true)
            {
                var result = await taskService.AddTask(dialog.TaskTitle, dialog.TaskDescription);
                AddBotMessage(result.message);
            }
        }

        private async void ShowTasksBtn_Click(object sender, RoutedEventArgs e)
        {
            string tasks = taskService.GetTasksDisplay(); // REMOVED await - returns string directly
            AddBotMessage(tasks);
        }

        private void QuizBtn_Click(object sender, RoutedEventArgs e)
        {
            var quizWindow = new QuizWindow(quizService);
            quizWindow.Owner = this;
            quizWindow.ShowDialog();
        }

        private async void LogBtn_Click(object sender, RoutedEventArgs e)
        {
            var entries = await logService.GetLogEntries();
            var logWindow = new ActivityLogWindow(entries);
            logWindow.Owner = this;
            logWindow.ShowDialog();
        }
    }
}