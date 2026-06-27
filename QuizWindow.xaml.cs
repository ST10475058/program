using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CybersecurityChatbotPOE.Services;
using CybersecurityChatbotPOE.Models;

namespace CybersecurityChatbotPOE.Views
{
    public partial class QuizWindow : Window
    {
        private QuizService quizService;
        private int selectedAnswerIndex = -1;

        public QuizWindow(QuizService service)
        {
            InitializeComponent();
            quizService = service;

            quizService.QuestionChanged += OnQuestionChanged;
            quizService.ScoreChanged += OnScoreChanged;
            quizService.Completed += OnQuizCompleted;

            quizService.StartQuiz();
        }

        private void OnQuestionChanged(object? sender, QuizQuestion question)
        {
            Dispatcher.Invoke(() =>
            {
                QuestionText.Text = question.Question;
                ProgressText.Text = $"Question {quizService.CurrentNumber} / {quizService.TotalQuestions}";

                double progress = (double)quizService.CurrentNumber / quizService.TotalQuestions * 100;
                ProgressBar.Value = progress;

                OptionsPanel.Children.Clear();
                selectedAnswerIndex = -1;
                NextButton.IsEnabled = false;

                for (int i = 0; i < question.Options.Count; i++)
                {
                    int index = i;
                    var button = new Button
                    {
                        Content = question.Options[i],
                        Height = 45,
                        Margin = new Thickness(0, 6, 0, 6),
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#415A77")),
                        Foreground = Brushes.White,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        Padding = new Thickness(20, 0, 0, 0),
                        FontSize = 14,
                        Cursor = System.Windows.Input.Cursors.Hand
                    };
                    button.Click += (s, e) => OnAnswerSelected(index);
                    OptionsPanel.Children.Add(button);
                }
            });
        }

        private void OnAnswerSelected(int index)
        {
            if (selectedAnswerIndex != -1) return;

            selectedAnswerIndex = index;
            string feedback = quizService.SubmitAnswer(index);

            MessageBox.Show(feedback, "Answer Feedback", MessageBoxButton.OK, MessageBoxImage.Information);
            NextButton.IsEnabled = true;
        }

        private void OnScoreChanged(object? sender, int score)
        {
            Dispatcher.Invoke(() =>
            {
                ScoreText.Text = $"Score: {score}";
            });
        }

        private void OnQuizCompleted(object? sender, QuizResult result)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(result.DisplayText, "Quiz Complete!", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            });
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAnswerIndex == -1)
            {
                MessageBox.Show("Please select an answer first!", "No Answer Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            quizService.NextQuestion();
        }

        protected override void OnClosed(EventArgs e)
        {
            quizService.QuestionChanged -= OnQuestionChanged;
            quizService.ScoreChanged -= OnScoreChanged;
            quizService.Completed -= OnQuizCompleted;
            base.OnClosed(e);
        }
    }
}