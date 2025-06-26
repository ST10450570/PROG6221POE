# PROG6221POE
Cybersecurity Awareness Chatbot
Overview
The Cybersecurity Awareness Chatbot is a Windows desktop application designed to educate users about cybersecurity concepts, provide security tips, and help users manage security-related tasks. The application features a conversational interface, quiz functionality, task management, and activity logging.

Folder Structure
CybersecurityChatbot/
├── Chatbot/                      # Main application folder
│   ├── ArtDisplay.cs             # ASCII art and visual elements
│   ├── ChatbotBase.cs            # Base chatbot functionality
│   ├── InputValidator.cs         # Input validation utilities
│   ├── IResponder.cs             # Interface for chatbot responses
│   ├── MainWindow.xaml           # Main application window (UI)
│   ├── MainWindow.xaml.cs        # Main application logic
│   ├── Quiz.cs                   # Quiz functionality
│   ├── SecurityChatbot.cs        # Cybersecurity-specific chatbot implementation
│   └── greeting.wav              # Welcome sound file
|   
└── README.md                     # This file

Setup Instructions
Prerequisites
Windows 10 or later

.NET 6.0 or later

Visual Studio 2022 (recommended)

Installation
Clone or download the repository

Open the solution file in Visual Studio

Restore NuGet packages if prompted

Build the solution (Ctrl+Shift+B)

Run the application (F5)
Configuration
The application will create necessary files automatically on first run

To change the greeting sound, replace greeting.wav in the project directory

Application settings are stored in %APPDATA%\CybersecurityChatbot\settings.json
Usage Instructions
Main Features
Chat Interface: Ask cybersecurity questions and get detailed responses

Task Management: Create and manage security-related tasks with reminders

Quiz Mode: Test your cybersecurity knowledge

Activity Log: Track your interactions with the chatbot

Basic Commands
Type your question in the input box and press Enter or click Send

Start with "help" to see available commands

Use "topics" to see available cybersecurity topics

Type "quiz" to start a cybersecurity quiz
Task Management
Add tasks using the side panel or by typing:

"Add task [title] with description [description]"

"Remind me to [task] in [time period]"

Complete tasks by clicking the checkmark (✓) button

Delete tasks by clicking the (×) button
Quiz Mode
Type "start quiz" to begin

Answer questions by selecting options in the side panel

View your score and feedback after each question

Examples
Chat Examples
text
Copy
Download
User: What is phishing?
Bot: 🎣 Phishing uses fake communications to steal data. Variants include spear phishing (targeted) and whaling (executive targets)...

User: Give me a password tip
Bot: 💡 Tip: Create memorable passphrases like 'CorrectHorseBatteryStaple' instead of complex passwords...

User: I'm worried about malware
Bot: I understand malware can be really scary with all those sophisticated variants going around...
Task Examples
text
Copy
Download
"Add task Update passwords with description Change all important account passwords"
"Remind me to backup files in 3 days"
"Create task called Check firewall settings"
Quiz Example
text
Copy
Download
Q: What should you do if you receive an email asking for your password?
1) Reply with your password
2) Delete the email
3) Report the email as phishing
4) Ignore it

You selected: 3) Report the email as phishing
Correct! Reporting phishing emails helps prevent scams and protects others.
Troubleshooting
Common Issues
Sound not playing: Ensure greeting.wav exists in the application directory

UI not updating: Try restarting the application

Tasks not saving: Check write permissions in the application data folder
