using System.Collections.Generic;
using System.Linq;

namespace Chatbot
{
    public class Quiz
    {
        public List<QuizQuestion> Questions { get; } = new List<QuizQuestion>();
        public int CurrentQuestionIndex { get; private set; } = 0;
        public int Score { get; private set; } = 0;

        public Quiz()
        {
            InitializeQuestions();
        }

        private void InitializeQuestions()
        {
            Questions.Add(new QuizQuestion
            {
                QuestionText = "What should you do if you receive an email asking for your password?",
                Options = new List<string> { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                CorrectAnswerIndex = 2,
                Explanation = "Reporting phishing emails helps prevent scams and protects others.",
                Type = QuestionType.MultipleChoice
            });

            Questions.Add(new QuizQuestion
            {
                QuestionText = "Is it safe to use the same password for multiple accounts?",
                Options = new List<string> { "True", "False" },
                CorrectAnswerIndex = 1,
                Explanation = "Using unique passwords for each account helps prevent widespread access if one password is compromised.",
                Type = QuestionType.TrueFalse
            });

            Questions.Add(new QuizQuestion
            {
                QuestionText = "Which of the following is the strongest password?",
                Options = new List<string> { "password123", "123456", "Qw!8$zT@4L", "mydogname" },
                CorrectAnswerIndex = 2,
                Explanation = "Strong passwords are long and use a mix of letters, numbers, and special characters.",
                Type = QuestionType.MultipleChoice
            });

            Questions.Add(new QuizQuestion
            {
                QuestionText = "True or False: HTTPS websites are generally safer to browse than HTTP websites.",
                Options = new List<string> { "True", "False" },
                CorrectAnswerIndex = 0,
                Explanation = "HTTPS encrypts data between your browser and the website, improving security.",
                Type = QuestionType.TrueFalse
            });

            Questions.Add(new QuizQuestion
            {
                QuestionText = "Which of the following is a sign of phishing?",
                Options = new List<string> { "Unexpected attachments", "Urgent demands", "Strange links", "All of the above" },
                CorrectAnswerIndex = 3,
                Explanation = "Phishing emails often include all these signs. Always verify before clicking.",
                Type = QuestionType.MultipleChoice
            });

            Questions.Add(new QuizQuestion
            {
                QuestionText = "True or False: Public Wi-Fi is always safe to use without protection.",
                Options = new List<string> { "True", "False" },
                CorrectAnswerIndex = 1,
                Explanation = "Public Wi-Fi is insecure. Use a VPN or avoid sensitive tasks on it.",
                Type = QuestionType.TrueFalse
            });

            Questions.Add(new QuizQuestion
            {
                QuestionText = "Which of the following is a secure practice?",
                Options = new List<string> { "Clicking links in random emails", "Downloading files from unverified sites", "Using multi-factor authentication", "Ignoring software updates" },
                CorrectAnswerIndex = 2,
                Explanation = "Multi-factor authentication adds an extra layer of security to your accounts.",
                Type = QuestionType.MultipleChoice
            });

            Questions.Add(new QuizQuestion
            {
                QuestionText = "True or False: It’s okay to share personal information on social media freely.",
                Options = new List<string> { "True", "False" },
                CorrectAnswerIndex = 1,
                Explanation = "Too much personal info can be used in scams or identity theft.",
                Type = QuestionType.TrueFalse
            });

            Questions.Add(new QuizQuestion
            {
                QuestionText = "Which one helps protect against malware?",
                Options = new List<string> { "Antivirus software", "Opening unknown files", "Ignoring updates", "None of the above" },
                CorrectAnswerIndex = 0,
                Explanation = "Antivirus software detects and removes harmful programs.",
                Type = QuestionType.MultipleChoice
            });

            Questions.Add(new QuizQuestion
            {
                QuestionText = "True or False: Social engineering is a cyberattack that relies on human interaction.",
                Options = new List<string> { "True", "False" },
                CorrectAnswerIndex = 0,
                Explanation = "Social engineering tricks people into giving away confidential information.",
                Type = QuestionType.TrueFalse
            });
        }

        public QuizQuestion GetCurrentQuestion()
        {
            if (Questions == null || Questions.Count == 0 || CurrentQuestionIndex >= Questions.Count)
                return null;

            return Questions[CurrentQuestionIndex];
        }


        public bool CheckAnswer(string selectedOption)
        {
            var currentQuestion = GetCurrentQuestion();
            bool isCorrect = currentQuestion.Options[currentQuestion.CorrectAnswerIndex] == selectedOption;
            if (isCorrect) Score++;
            return isCorrect;
        }

        public bool MoveToNextQuestion()
        {
            if (CurrentQuestionIndex < Questions.Count - 1)
            {
                CurrentQuestionIndex++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            CurrentQuestionIndex = 0;
            Score = 0;
        }

        public string GetFinalMessage()
        {
            if (Score >= 8)
                return $"Great job! You’re a cybersecurity pro! Final Score: {Score}/{Questions.Count}";
            else if (Score >= 5)
                return $"Good effort! Keep practicing to improve. Final Score: {Score}/{Questions.Count}";
            else
                return $"Keep learning to stay safe online! Final Score: {Score}/{Questions.Count}";
        }
    }

    public class QuizQuestion
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string Explanation { get; set; }
        public QuestionType Type { get; set; } = QuestionType.MultipleChoice;

        public string Feedback => Explanation;
    }

    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse
    }
}
