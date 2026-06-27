using System.Collections.Generic;
using System.Windows;
using CybersecurityChatbotPOE.Models;

namespace CybersecurityChatbotPOE.Views
{
    public partial class ActivityLogWindow : Window
    {
        public ActivityLogWindow(List<ActivityLogEntry> entries)
        {
            InitializeComponent();
            LogItemsControl.ItemsSource = entries;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}