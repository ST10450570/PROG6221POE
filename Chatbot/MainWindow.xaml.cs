﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Chatbot
{
    public partial class MainWindow : Window
    {
        private SecurityChatbot _chatbot;
        private DispatcherTimer _reminderTimer;
        private List<TaskItem> _tasks = new List<TaskItem>();
        private List<TaskItem> _completedTasks = new List<TaskItem>();
        private List<ActivityLogEntry> _activityLog = new List<ActivityLogEntry>();
        private Quiz _quizGame;
        private bool _showingCompletedTasks = false;
        private bool _awaitingTaskTitle = false;
        private bool _awaitingName = false;
        private bool _awaitingTaskDescription = false;
        private bool _awaitingTaskReminder = false;
        private string _currentTaskTitle = "";
        private string _currentTaskDescription = "";
        





        public MainWindow()
        {
            InitializeComponent();
            InitializeChatbot();
            InitializeQuiz();
            SetupReminderTimer();
            UpdateTaskListDisplay();
            UpdateActivityLog();
            UserInfoText.Text = "User";
            _awaitingName = true;
            AppendToChat("🤖 Welcome to the Cybersecurity Awareness Chatbot! What's your name?", Brushes.Magenta);
        }

        private void InitializeChatbot()
        {
            try
            {
                string audioPath = @"C:\Users\chuma\OneDrive\Desktop\PROG6221POE\Chatbot\greeting.wav";
                _chatbot = new SecurityChatbot(audioPath);
                _chatbot.Greet(this);
                AddToActivityLog("Application started", "System");
            }
            catch (Exception ex)
            {
                AppendToChat("▲ Error initializing chatbot: " + ex.Message, Brushes.Red);
            }
        }

        private void InitializeQuiz()
        {
            _quizGame = new Quiz();
        }

        private void SetupReminderTimer()
        {
            _reminderTimer = new DispatcherTimer();
            _reminderTimer.Interval = TimeSpan.FromMinutes(1);
            _reminderTimer.Tick += CheckReminders;
            _reminderTimer.Start();
        }

        private void CheckReminders(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            foreach (var task in _tasks.Where(t => t.ReminderDate.HasValue && t.ReminderDate <= now && !t.ReminderTriggered))
            {
                task.ReminderTriggered = true;
                AppendToChat($"▲ Reminder: {task.Title} - {task.Description}", Brushes.Orange);
                AddToActivityLog($"Reminder triggered for task: {task.Title}", "System");
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserInput();
        }

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUserInput();
            }
        }

        private void ProcessUserInput()
        {
            string input = UserInput.Text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                return;
            }

            AppendToChat($"{_chatbot.Username}: {input}", Brushes.LightBlue);
            UserInput.Clear();

            try
            {
                if (_awaitingName)
                {
                    HandleNameInput(input);
                    return;
                }

                if (_awaitingTaskTitle)
                {
                    HandleTaskTitleInput(input);
                    return;
                }

                if (_awaitingTaskDescription)
                {
                    HandleTaskDescriptionInput(input);
                    return;
                }

                if (_awaitingTaskReminder)
                {
                    HandleTaskReminderInput(input);
                    return;
                }

                // Try to parse as task command first
                if (TryParseTaskCommand(input))
                {
                    return;
                }

                // Handle quiz commands before special commands
                if (HandleQuizCommands(input))
                {
                    return;
                }

                if (HandleSpecialCommands(input))
                {
                    return;
                }

                // Fallback to chatbot response
                _chatbot.Respond(input, this);
            }
            catch (Exception ex)
            {
                AppendToChat($"▲ Error: {ex.Message}", Brushes.Red);
            }
        }

        private bool HandleQuizCommands(string input)
        {
            if (input.Equals("start quiz", StringComparison.OrdinalIgnoreCase))
            {
                StartQuizButton_Click(null, null);
                return true;
            }

            if (input.Equals("next question", StringComparison.OrdinalIgnoreCase) && NextQuizButton.IsEnabled)
            {
                NextQuizButton_Click(null, null);
                return true;
            }

            if (input.Equals("submit answer", StringComparison.OrdinalIgnoreCase) && SubmitQuizButton.IsEnabled)
            {
                SubmitQuizButton_Click(null, null);
                return true;
            }

            // Handle direct quiz answers
            if (QuizOption1.IsEnabled && (input.Equals("a", StringComparison.OrdinalIgnoreCase) || input == "1" || input=="True"))
            {
                QuizOption1.IsChecked = true;
                SubmitQuizButton_Click(null, null);
                return true;
            }

            if (QuizOption2.IsEnabled && (input.Equals("b", StringComparison.OrdinalIgnoreCase) || input == "2" || input == "True"))
            {
                QuizOption2.IsChecked = true;
                SubmitQuizButton_Click(null, null);
                return true;
            }

            if (QuizOption3.IsEnabled && (input.Equals("c", StringComparison.OrdinalIgnoreCase) || input == "3"))
            {
                QuizOption3.IsChecked = true;
                SubmitQuizButton_Click(null, null);
                return true;
            }

            if (QuizOption4.IsEnabled && (input.Equals("d", StringComparison.OrdinalIgnoreCase) || input == "4"))
            {
                QuizOption4.IsChecked = true;
                SubmitQuizButton_Click(null, null);
                return true;
            }

            return false;
        }


        private void HandleNameInput(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                AppendToChat("🤖 Please enter a valid name.", Brushes.Magenta);
                return;
            }

            _chatbot.Username = name;
            UserInfoText.Text = name;
            _awaitingName = false;
            AppendToChat($"🤖 Nice to meet you, {name}! How can I help you with cybersecurity today?", Brushes.Magenta);
            AddToActivityLog($"User set name to: {name}", "User");
        }

        private void HandleTaskTitleInput(string title)
        {
            _currentTaskTitle = title;
            _awaitingTaskTitle = false;
            AppendToChat("🤖 Please enter the task description:", Brushes.Magenta);
        }

        private void HandleTaskDescriptionInput(string description)
        {
            _currentTaskDescription = description;
            _awaitingTaskDescription = false;
            _awaitingTaskReminder = true;
            AppendToChat("🤖 Would you like to set a reminder? (yes/no or specify days, e.g. '3 days')", Brushes.Magenta);
        }

        private bool TryParseTaskCommand(string input)
        {
            // Check if input matches any task creation pattern
            if (Regex.IsMatch(input, @"\b(add|create|new)\s+task\b", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(input, @"\b(remind\s+me\s+to|set\s+reminder\s+for)\b", RegexOptions.IgnoreCase))
            {
                // Start interactive task creation if details are incomplete
                if (!TryParseCompleteTaskCommand(input))
                {
                    if (!_awaitingTaskDescription) // Only call if not already waiting for description
                        StartInteractiveTaskCreation(input);
                }

                return true;
            }
            return false;
        }

        private bool TryParseCompleteTaskCommand(string input)
        {
            // First try to extract complete task details from command
            var match = Regex.Match(input,
                @"(?:(?:add|create)\s+task\s+(?:called\s+)?(?<title>.+?)(?:\s+with\s+description\s+(?<desc>.+?))?(?:\s+(?:in|for|on|by)\s+(?<time>.+))?|" +
                @"(?:remind\s+me\s+to|set\s+reminder\s+for)\s+(?<title>.+?)(?:\s+(?:with\s+details?|description)\s+(?<desc>.+?))?(?:\s+(?:in|for|on|by)\s+(?<time>.+))?)",
                RegexOptions.IgnoreCase);

            if (match.Success)
            {
                // PROPERLY extract the full title without truncation
                string title = match.Groups["title"].Value.Trim();

                // Ensure we capture everything after "Add Task" until description/time markers
                if (title.Length == 1 && input.Contains("Add Task") )
                {
                    // Fallback for when regex doesn't capture full title
                    int taskIndex = input.IndexOf("Add Task", StringComparison.OrdinalIgnoreCase);
                    title = input.Substring(taskIndex + 8).Trim();

                    // Remove any trailing description/time phrases
                    var descIndex = title.IndexOf(" with ", StringComparison.OrdinalIgnoreCase);
                    if (descIndex > 0) title = title.Substring(0, descIndex).Trim();

                    var timeIndex = title.IndexOf(" in ", StringComparison.OrdinalIgnoreCase);
                    if (timeIndex > 0) title = title.Substring(0, timeIndex).Trim();
                }

                string description = match.Groups["desc"].Success ? match.Groups["desc"].Value.Trim() : "";
                string timeExpression = match.Groups["time"].Success ? match.Groups["time"].Value.Trim() : "";

                DateTime? reminderDate = ParseReminderTime(timeExpression);

                // If title is still just one character, fall back to interactive mode
                if (title.Length <= 1)
                {
                    StartInteractiveTaskCreation(input);
                    return false;
                }


                if (string.IsNullOrWhiteSpace(description))
                {
                    _currentTaskTitle = title;
                   
                    AppendToChat($"🤖 Please describe the task '{title}':", Brushes.Magenta);
                   _awaitingTaskDescription = true; // Set this here instead
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(timeExpression) && !reminderDate.HasValue)
                {
                    _currentTaskTitle = title;
                    _currentTaskDescription = description;
                    _awaitingTaskReminder = true;
                    AppendToChat($"🤖 I didn't understand the time '{timeExpression}'. When should I remind you? (e.g., 'in 3 days', 'tomorrow')", Brushes.Magenta);
                    return false;
                }

                CreateTask(title, description, reminderDate);
                return true;
            }

            return false;
        }

        private void StartInteractiveTaskCreation(string input)
        {
            // Try to extract at least a title from the input
            string potentialTitle = Regex.Replace(input,
                @"\b(add|create|new)\s+task\b|\b(remind\s+me\s+to|set\s+reminder\s+for)\b",
                "", RegexOptions.IgnoreCase).Trim();

            if (!string.IsNullOrWhiteSpace(potentialTitle))
            {
                _currentTaskTitle = potentialTitle;
                AppendToChat($"🤖 Please describe the task '{potentialTitle}':", Brushes.Magenta);
                _awaitingTaskDescription = true; // Set this here
            }
            else
            {
                _awaitingTaskTitle = true;
                AppendToChat("🤖 What would you like to name this task?", Brushes.Magenta);
            }
        }

        private DateTime? ParseReminderTime(string timeExpression)
        {
            if (string.IsNullOrWhiteSpace(timeExpression))
                return null;

            timeExpression = timeExpression.ToLower();

            // Handle simple expressions first
            if (timeExpression == "tomorrow")
                return DateTime.Now.AddDays(1);
            if (timeExpression == "next week")
                return DateTime.Now.AddDays(7);
            if (timeExpression == "next month")
                return DateTime.Now.AddMonths(1);

            // Handle "in X days/weeks/months"
            var match = Regex.Match(timeExpression, @"(?:in|for)\s+(\d+)\s+(day|week|month)s?");
            if (match.Success)
            {
                int quantity = int.Parse(match.Groups[1].Value);
                string unit = match.Groups[2].Value;

                
                switch (unit)
                {
                    case "day":
                        return DateTime.Now.AddDays(quantity);
                    case "week":
                        return DateTime.Now.AddDays(quantity * 7);
                    case "month":
                        return DateTime.Now.AddMonths(quantity);
                    default:
                        return null;
                }
            }

            // Handle specific dates
            if (DateTime.TryParse(timeExpression, out DateTime specificDate))
            {
                return specificDate;
            }

            return null;
        }

        private void CreateTask(string title, string description, DateTime? reminderDate)
        {
            var task = new TaskItem
            {
                Title = title,
                Description = string.IsNullOrWhiteSpace(description) ? "No description provided" : description,
                ReminderDate = reminderDate,
                CreatedDate = DateTime.Now,
                IsCompleted = false
            };

            _tasks.Add(task);

            string response = $"✅ Task created: \"{task.Title}\"\n";
            response += $"📝 Description: {task.Description}\n";
            if (task.ReminderDate.HasValue)
            {
                response += $"⏰ Reminder set for: {task.ReminderDate.Value.ToString("f")}";
            }

            AppendToChat(response, Brushes.LightGreen);
            AddToActivityLog($"Task created:\" {task.Title}", "User");
            UpdateTaskListDisplay();

            // Update UI to match
            Dispatcher.Invoke(() =>
            {
                TaskTitleInput.Text = title;
                TaskDescriptionInput.Text = description;
                if (reminderDate.HasValue)
                {
                    TaskReminderDate.SelectedDate = reminderDate;
                    SetReminderCheck.IsChecked = true;
                }
            });
        }

        private bool HandleSpecialCommands(string input)
        {
            if (input.Equals("topics", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("list topics", StringComparison.OrdinalIgnoreCase))
            {
                ShowAvailableTopics();
                return true;
            }

            if (input.Equals("show activity log", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("what have i done", StringComparison.OrdinalIgnoreCase))
            {
                ShowActivityLog();
                return true;
            }

            if (input.Equals("clear chat", StringComparison.OrdinalIgnoreCase))
            {
                ChatDisplay.Document.Blocks.Clear();
                return true;
            }

            if (input.StartsWith("add task", StringComparison.OrdinalIgnoreCase) ||
                input.StartsWith("create task", StringComparison.OrdinalIgnoreCase))
            {
                HandleAddTaskCommand(input);
                return true;
            }

            if (input.StartsWith("set username", StringComparison.OrdinalIgnoreCase))
            {
                HandleSetUsernameCommand(input);
                return true;
            }

            if (input.StartsWith("set reminder for 3 days", StringComparison.OrdinalIgnoreCase))
            {
                TaskReminderDate.SelectedDate = DateTime.Now.AddDays(3);
                SetReminderCheck.IsChecked = true;
                AppendToChat("Reminder set for 3 days from now", Brushes.LightGreen);
                return true;
            }

            return false;
        }

        private void StartTaskCreation()
        {
            _awaitingTaskTitle = true;
            AppendToChat("🤖 Please enter the task title:", Brushes.Magenta);
        }

        private void DisplayRecentActivity()
        {
            var recentLogs = _activityLog
                .OrderByDescending(x => x.Timestamp)
                .Take(5)
                .ToList();

            AppendToChat("📜 Recent Activity Log:", Brushes.White);
            foreach (var log in recentLogs)
            {
                AppendToChat($"• {log.Timestamp.ToShortTimeString()}: {log.Action}", Brushes.LightGray);
            }

            UpdateActivityLog(); 
        }
        private void HandleAddTaskCommand(string input)
        {
            string taskDetails = input.Substring(input.IndexOf("task", StringComparison.OrdinalIgnoreCase) + 4);
            TaskTitleInput.Text = taskDetails.Trim();
            TaskDescriptionInput.Focus();
            AppendToChat("Please enter task details in the side panel", Brushes.Yellow);
        }

        private void HandleSetUsernameCommand(string input)
        {
            string newUsername = input.Substring(input.IndexOf("username", StringComparison.OrdinalIgnoreCase) + 8).Trim();
            if (!string.IsNullOrEmpty(newUsername))
            {
                _chatbot.Username = newUsername;
                UserInfoText.Text = newUsername;
                AppendToChat($"Username set to: {newUsername}", Brushes.LightGreen);
                AddToActivityLog($"Username changed to: {newUsername}", "User");
            }
        }

        private void HandleTaskReminderInput(string input)
        {
            DateTime? reminderDate = null;
            input = input.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(_currentTaskTitle))
            {
                AppendToChat("⚠️ Task title is missing. Cannot create task.", Brushes.OrangeRed);
                return;
            }

            if (input == "yes" || input == "y")
            {
                reminderDate = DateTime.Now.AddDays(1);
            }
            else if (input.StartsWith("in "))
            {
                // Example: "in 2 days", "in 5", "in 1 week"
                var parts = input.Substring(3).Split(' ');
                if (int.TryParse(parts[0], out int amount))
                {
                    string unit = parts.Length > 1 ? parts[1] : "days";

                    switch (unit)
                    {
                        case "day":
                        case "days":
                            reminderDate = DateTime.Now.AddDays(amount);
                            break;
                        case "week":
                        case "weeks":
                            reminderDate = DateTime.Now.AddDays(amount * 7);
                            break;
                        case "month":
                        case "months":
                            reminderDate = DateTime.Now.AddMonths(amount);
                            break;
                        default:
                            AppendToChat("⚠️ Unrecognized time unit. Try 'in 3 days', 'in 1 week', etc.", Brushes.OrangeRed);
                            return;
                    }
                }
                else
                {
                    AppendToChat("⚠️ Could not understand the time format. Try 'in 2 days', 'in 1 week', etc.", Brushes.OrangeRed);
                    return;
                }
            }
            else if (DateTime.TryParse(input, out DateTime parsedDate))
            {
                reminderDate = parsedDate;
            }
            else if (!string.IsNullOrWhiteSpace(input) && input != "no" && input != "n")
            {
                AppendToChat("⚠️ Invalid input. Please enter a date (e.g., 'in 3 days', 'next week', or 'no').", Brushes.OrangeRed);
                return;
            }

            // Create the task
            var task = new TaskItem
            {
                Title = _currentTaskTitle.Trim(),
                Description = string.IsNullOrWhiteSpace(_currentTaskDescription) ? "No description provided." : _currentTaskDescription.Trim(),
                ReminderDate = reminderDate,
                CreatedDate = DateTime.Now,
                IsCompleted = false
            };

            _tasks.Add(task);

            AppendToChat($"✅ Task added: {task.Title}" +
                (task.ReminderDate.HasValue ? $" (⏰ Reminder: {task.ReminderDate.Value.ToString("g")})" : ""),
                Brushes.LightGreen);

            AddToActivityLog($"Task added: {task.Title}", "User");
            UpdateTaskListDisplay();

            // Reset
            _awaitingTaskReminder = false;
            _currentTaskTitle = "";
            _currentTaskDescription = "";
        }

        private void ShowAvailableTopics()
        {
            var topics = _chatbot.GetAvailableTopics();
            AppendToChat("Available topics:\n" + string.Join(". ", topics), Brushes.LightGreen);
        }

        private void ShowActivityLog()
        {
            var recentLogs = _activityLog
                .OrderByDescending(x => x.Timestamp)
                .Take(5)
                .ToList();

            AppendToChat("Recent Activity Log:", Brushes.White);
            foreach (var log in recentLogs)
            {
                AppendToChat($"• {log.Timestamp.ToShortTimeString()}: {log.Action}", Brushes.White);
            }

            UpdateActivityLog();
            AppendToChat("Activity log displayed in side panel", Brushes.LightGreen);
        }

        public void AppendToChat(string message, Brush color)
        {
            Dispatcher.Invoke(() =>
            {
                var paragraph = new Paragraph(new Run(message))
                {
                    Margin = new Thickness(0, 6, 0, 6),
                    Foreground = color
                };
                ChatDisplay.Document.Blocks.Add(paragraph);
                ChatDisplay.ScrollToEnd();
            });
        }

        public void AddToActivityLog(string action, string source)
        {
            _activityLog.Add(new ActivityLogEntry
            {
                Timestamp = DateTime.Now,
                Action = action,
                Source = source
            });

            if (_activityLog.Count > 50)
            {
                _activityLog.RemoveAt(0);
            }

            UpdateActivityLog();
        }

        private void UpdateActivityLog()
        {
            Dispatcher.Invoke(() =>
            {
                ActivityLogList.ItemsSource = null;
                ActivityLogList.ItemsSource = _activityLog.OrderByDescending(x => x.Timestamp).Take(10).ToList();
            });
        }

        private void UpdateTaskListDisplay()
        {
            Dispatcher.Invoke(() =>
            {
                TasksList.ItemsSource = null;
                TasksList.ItemsSource = _showingCompletedTasks ?
                    _tasks.Concat(_completedTasks).ToList() :
                    _tasks;
            });
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskTitleInput.Text))
            {
                AppendToChat("Task title cannot be empty", Brushes.Red);
                return;
            }

            var task = new TaskItem
            {
                Title = TaskTitleInput.Text,
                Description = TaskDescriptionInput.Text,
                ReminderDate = SetReminderCheck.IsChecked == true ? TaskReminderDate.SelectedDate : null,
                CreatedDate = DateTime.Now,
                IsCompleted = false
            };

            _tasks.Add(task);
            TaskTitleInput.Clear();
            TaskDescriptionInput.Clear();
            TaskReminderDate.SelectedDate = null;
            SetReminderCheck.IsChecked = false;

            AppendToChat($"Task added: {task.Title}" +
                (task.ReminderDate.HasValue ? $" (Reminder set for {task.ReminderDate.Value.ToString()})" : ""),
                Brushes.LightGreen);
            AddToActivityLog($"Task added: {task.Title}", "User");
            UpdateTaskListDisplay();
        }

        private void CompleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is TaskItem task)
            {
                task.IsCompleted = true;
                task.CompletedDate = DateTime.Now;
                _tasks.Remove(task);
                _completedTasks.Add(task);

                AppendToChat($"Task completed: {task.Title}", Brushes.LightGreen);
                AddToActivityLog($"Task completed: {task.Title}", "User");
                UpdateTaskListDisplay();
            }
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is TaskItem task)
            {
                if (_tasks.Contains(task))
                {
                    _tasks.Remove(task);
                }
                else if (_completedTasks.Contains(task))
                {
                    _completedTasks.Remove(task);
                }

                AppendToChat($"Task deleted: {task.Title}", Brushes.Orange);
                AddToActivityLog($"Task deleted: {task.Title}", "User");
                UpdateTaskListDisplay();
            }
        }

        private void ShowCompletedTasks_Click(object sender, RoutedEventArgs e)
        {
            _showingCompletedTasks = true;
            UpdateTaskListDisplay();
            AppendToChat("Showing all tasks (including completed)", Brushes.LightGreen);
        }

        private void HideCompletedTasks_Click(object sender, RoutedEventArgs e)
        {
            _showingCompletedTasks = false;
            UpdateTaskListDisplay();
            AppendToChat("Hiding completed tasks", Brushes.LightGreen);
        }

        private void SetReminderCheck_Checked(object sender, RoutedEventArgs e)
        {
            TaskReminderDate.IsEnabled = true;
        }

        private void SetReminderCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            TaskReminderDate.IsEnabled = false;
        }

        private void StartQuizButton_Click(object sender, RoutedEventArgs e)
        {
            _quizGame.Reset();
            DisplayCurrentQuestion();
            StartQuizButton.IsEnabled = false;
            SubmitQuizButton.IsEnabled = true;
            NextQuizButton.IsEnabled = false;
            QuizFeedbackText.Text = "";
            QuizScoreText.Text = "";
            AppendToChat("Quiz started! Answer the questions in the side panel.", Brushes.LightGreen);
            AddToActivityLog("Quiz started", "User");
        }

        private void SubmitQuizButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedOption = GetSelectedOption();
            if (selectedOption == null)
            {
                QuizFeedbackText.Text = "Please select an answer!";
                QuizFeedbackText.Foreground = Brushes.Orange;
                return;
            }

            bool isCorrect = _quizGame.CheckAnswer(selectedOption);
            string feedback = _quizGame.GetCurrentQuestion().Feedback;
            QuizFeedbackText.Text = isCorrect ?
                $"Correct! {feedback}" :
                $"Incorrect. {feedback}";
            QuizFeedbackText.Foreground = isCorrect ? Brushes.LightGreen : Brushes.Orange;
            QuizScoreText.Text = $"Score: {_quizGame.Score}/{_quizGame.CurrentQuestionIndex + 1}";
            QuizScoreText.Foreground = Brushes.White;
            SubmitQuizButton.IsEnabled = false;
            NextQuizButton.IsEnabled = true;
            AddToActivityLog($"Answered quiz question: {_quizGame.GetCurrentQuestion().QuestionText}", "User");
        }

        private void NextQuizButton_Click(object sender, RoutedEventArgs e)
        {
            if (_quizGame.MoveToNextQuestion())
            {
                DisplayCurrentQuestion();
                SubmitQuizButton.IsEnabled = true;
                NextQuizButton.IsEnabled = false;
                QuizFeedbackText.Text = "";
            }
            else
            {
                QuizCompleted();
            }
        }

        private void DisplayCurrentQuestion()
        {
            if (_quizGame.Questions.Count == 0)
            {
                QuizQuestionText.Text = "No questions available.";
                return;
            }

            if (_quizGame.CurrentQuestionIndex >= _quizGame.Questions.Count)
            {
                QuizCompleted();
                return;
            }

            var question = _quizGame.GetCurrentQuestion();
            QuizQuestionText.Text = question.QuestionText;

            QuizOption1.IsChecked = false;
            QuizOption2.IsChecked = false;
            QuizOption3.IsChecked = false;
            QuizOption4.IsChecked = false;

            QuizOption3.Visibility = question.Options.Count > 2 ? Visibility.Visible : Visibility.Collapsed;
            QuizOption4.Visibility = question.Options.Count > 3 ? Visibility.Visible : Visibility.Collapsed;

            QuizOption1.Content = question.Options.ElementAtOrDefault(0) ?? "";
            QuizOption2.Content = question.Options.ElementAtOrDefault(1) ?? "";
            QuizOption3.Content = question.Options.ElementAtOrDefault(2) ?? "";
            QuizOption4.Content = question.Options.ElementAtOrDefault(3) ?? "";
        }

        private string GetSelectedOption()
        {
            if (QuizOption1.IsChecked == true) return QuizOption1.Content.ToString();
            if (QuizOption2.IsChecked == true) return QuizOption2.Content.ToString();
            if (QuizOption3.IsChecked == true) return QuizOption3.Content.ToString();
            if (QuizOption4.IsChecked == true) return QuizOption4.Content.ToString();
            return null;
        }

        private void QuizCompleted()
        {
            double percentage = (double)_quizGame.Score / _quizGame.Questions.Count * 100;
            string message;
            if (percentage >= 80) message = "Excellent! You're a cybersecurity expert!";
            else if (percentage >= 60) message = "Good job! You know quite a bit about cybersecurity.";
            else if (percentage >= 40) message = "Not bad! Keep learning to improve your score.";
            else message = "Keep studying cybersecurity to stay safe online!";

            QuizQuestionText.Text = "Quiz completed!";
            QuizOption1.Content = "";
            QuizOption2.Content = "";
            QuizOption3.Content = "";
            QuizOption4.Content = "";
            QuizFeedbackText.Text = message;
            QuizScoreText.Text = $"Final score: {_quizGame.Score}/{_quizGame.Questions.Count}";
            QuizScoreText.Foreground = Brushes.Yellow;
            StartQuizButton.IsEnabled = true;
            SubmitQuizButton.IsEnabled = false;
            NextQuizButton.IsEnabled = false;

            AppendToChat($"Quiz completed. Score: {_quizGame.Score}/{_quizGame.Questions.Count} - {message}", Brushes.LightGreen);
            AddToActivityLog($"Quiz completed. Score: {_quizGame.Score}/{_quizGame.Questions.Count}", "User");
        }

        private void RefreshLogButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateActivityLog();
            AppendToChat("Activity log refreshed", Brushes.LightGreen);
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            _activityLog.Clear();
            UpdateActivityLog();
            AppendToChat("Activity log cleared", Brushes.Orange);
            AddToActivityLog("Activity log cleared", "User");
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void RemovePlaceholderText(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Name == "TaskTitleInput" && textBox.Text == "Task title...")
                {
                    textBox.Text = "";
                    textBox.Foreground = Brushes.White;
                }
                else if (textBox.Name == "TaskDescriptionInput" && textBox.Text == "Description...")
                {
                    textBox.Text = "";
                    textBox.Foreground = Brushes.White;
                }
            }
        }

        private void AddPlaceholderText(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Name == "TaskTitleInput" && string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = "Task title...";
                    textBox.Foreground = Brushes.Gray;
                }
                else if (textBox.Name == "TaskDescriptionInput" && string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = "Description...";
                    textBox.Foreground = Brushes.Gray;
                }
            }
        }
    }

    public class TaskItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool ReminderTriggered { get; set; }
        public string ReminderText => ReminderDate.HasValue ?
            $"  Remember: {ReminderDate.Value.ToString()}" :
            "No reminder set";
    }

    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Source { get; set; }

        public override string ToString()
        {
            return $"{Timestamp.ToShortTimeString()}: {Action}";
        }
    }
}