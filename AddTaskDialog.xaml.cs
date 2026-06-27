using System.Windows;

namespace CybersecurityChatbotPOE.Views
{
    public partial class AddTaskDialog : Window
    {
        public string TaskTitle => TitleTextBox.Text.Trim();
        public string TaskDescription => string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ? "No description" : DescriptionTextBox.Text.Trim();
        public int ReminderDays => 0;

        public AddTaskDialog()
        {
            InitializeComponent();
            TitleTextBox.Focus();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskTitle))
            {
                MessageBox.Show("Please enter a task title.", "Missing Information",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}