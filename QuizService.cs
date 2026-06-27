using System;
using System.Collections.Generic;
using System.Linq;
using CybersecurityChatbotPOE.Models;

namespace CybersecurityChatbotPOE.Services
{
    public class QuizService
    {
        private List<QuizQuestion> questions;
        private int currentIndex = 0;
        private int score = 0;
        private bool active = false;
        private DatabaseService? db;

        public event EventHandler<QuizQuestion>? QuestionChanged;
        public event EventHandler<int>? ScoreChanged;
        public event EventHandler<QuizResult>? Completed;

        public QuizService(DatabaseService? dbService = null)
        {
            db = dbService;
            LoadQuestions();
        }

        private void LoadQuestions()
        {
            questions = new List<QuizQuestion>
            {
                new QuizQuestion {
                    Question = "What does phishing refer to in cybersecurity?",
                    Options = new List<string> { "A) Fishing for compliments online", "B) A fraudulent attempt to obtain sensitive information", "C) A type of computer virus", "D) A secure way to send emails" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Phishing is when attackers pose as legitimate entities to steal passwords, credit card numbers, or other sensitive data."
                },
                new QuizQuestion {
                    Question = "Which of the following is an example of a strong password?",
                    Options = new List<string> { "A) password123", "B) PurpleElephant$Jumps!3Times", "C) 12345678", "D) qwerty" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Strong passwords use a mix of uppercase, lowercase, numbers, symbols, and are at least 12 characters long."
                },
                new QuizQuestion {
                    Question = "What does 2FA stand for?",
                    Options = new List<string> { "A) Two Factor Authentication", "B) Two Frequency Analysis", "C) Triple Factor Authentication", "D) Second Form Access" },
                    CorrectAnswerIndex = 0,
                    Explanation = "2FA adds an extra layer of security by requiring a second verification method beyond just your password."
                },
                new QuizQuestion {
                    Question = "True or False: Public WiFi is safe for online banking.",
                    Options = new List<string> { "A) True", "B) False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "False! Public WiFi networks are often unencrypted, allowing attackers to intercept your data."
                },
                new QuizQuestion {
                    Question = "What should you do if you receive a suspicious email asking for your password?",
                    Options = new List<string> { "A) Reply with your password", "B) Click the link to verify", "C) Report it as phishing and delete it", "D) Forward it to your friends" },
                    CorrectAnswerIndex = 2,
                    Explanation = "Never share passwords via email. Report phishing attempts and delete the email immediately."
                },
                new QuizQuestion {
                    Question = "What is a common sign of a phishing email?",
                    Options = new List<string> { "A) Professional design", "B) Poor grammar and spelling errors", "C) Your full name is used", "D) Comes from a known contact" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Poor grammar, urgent language, and mismatched sender addresses are common red flags for phishing emails."
                },
                new QuizQuestion {
                    Question = "What is SIM swap fraud?",
                    Options = new List<string> { "A) Trading SIM cards with friends", "B) When scammers transfer your phone number to their SIM", "C) A phone upgrade program", "D) A type of mobile antivirus" },
                    CorrectAnswerIndex = 1,
                    Explanation = "SIM swap fraud is when attackers convince your mobile provider to transfer your number to their SIM."
                },
                new QuizQuestion {
                    Question = "True or False: You should use the same password for multiple accounts to remember them easily.",
                    Options = new List<string> { "A) True", "B) False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "False! Reusing passwords means if one account is compromised, all your accounts are at risk."
                },
                new QuizQuestion {
                    Question = "What does 'https://' indicate?",
                    Options = new List<string> { "A) The website is slow", "B) The connection is encrypted and secure", "C) The website is free", "D) The website is from a government" },
                    CorrectAnswerIndex = 1,
                    Explanation = "HTTPS encrypts data between your browser and the website, protecting it from interception."
                },
                new QuizQuestion {
                    Question = "Which of these is a legitimate password manager?",
                    Options = new List<string> { "A) Bitwarden", "B) Password123", "C) Notepad", "D) Excel" },
                    CorrectAnswerIndex = 0,
                    Explanation = "Bitwarden, LastPass, and 1Password are legitimate password managers."
                },
                new QuizQuestion {
                    Question = "What is ransomware?",
                    Options = new List<string> { "A) A type of antivirus", "B) Malware that encrypts your files and demands payment", "C) A backup solution", "D) A password recovery tool" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Ransomware is malicious software that locks your files and demands payment to unlock them."
                },
                new QuizQuestion {
                    Question = "True or False: You should never share your OTP (One-Time Password) with anyone.",
                    Options = new List<string> { "A) True", "B) False" },
                    CorrectAnswerIndex = 0,
                    Explanation = "True! OTPs are for your use only. No legitimate organization will ever ask for your OTP."
                }
            };
        }

        public void StartQuiz()
        {
            currentIndex = 0;
            score = 0;
            active = true;
            var rnd = new Random();
            questions = questions.OrderBy(x => rnd.Next()).ToList();
            db?.AddLog("Quiz Started", "User started quiz");
            QuestionChanged?.Invoke(this, questions[0]);
        }

        public string SubmitAnswer(int selectedIndex)
        {
            if (!active) return "Quiz not active";
            var current = questions[currentIndex];
            bool isCorrect = selectedIndex == current.CorrectAnswerIndex;
            if (isCorrect)
            {
                score++;
                ScoreChanged?.Invoke(this, score);
            }
            return isCorrect ? $"✅ Correct! {current.Explanation}" : $"❌ Incorrect! The correct answer was: {current.Options[current.CorrectAnswerIndex]}\n\n{current.Explanation}";
        }

        public QuizQuestion? NextQuestion()
        {
            currentIndex++;
            if (currentIndex < questions.Count)
            {
                var next = questions[currentIndex];
                QuestionChanged?.Invoke(this, next);
                return next;
            }
            else
            {
                active = false;
                var result = new QuizResult
                {
                    Score = score,
                    Total = questions.Count,
                    Percentage = (double)score / questions.Count * 100,
                    Feedback = score >= 8 ? "🏆 EXCELLENT! You're a cybersecurity pro!" : score >= 6 ? "📚 Good job! Keep learning!" : "📖 Keep studying! Cybersecurity is important for everyone."
                };
                db?.AddLog("Quiz Completed", $"Score: {score}/{questions.Count}");
                Completed?.Invoke(this, result);
                return null;
            }
        }

        public QuizQuestion? GetCurrentQuestion() => currentIndex < questions.Count ? questions[currentIndex] : null;
        public bool IsActive => active;
        public int TotalQuestions => questions.Count;
        public int CurrentScore => score;
        public int CurrentNumber => currentIndex + 1;
    }

    public class QuizResult
    {
        public int Score { get; set; }
        public int Total { get; set; }
        public double Percentage { get; set; }
        public string Feedback { get; set; } = "";
        public string DisplayText => $"🎯 Score: {Score}/{Total} ({Percentage:F0}%)\n\n{Feedback}";
    }
}