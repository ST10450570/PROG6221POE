using System;

namespace Chatbot
{
    public static class InputValidator
    {
        public static bool IsValid(string input)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(input) && input.Length > 1;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsExitCommand(string input)
        {
            string[] exitCommands = { "exit", "quit", "leave", "stop", "end chat", "bye" };
            foreach (var cmd in exitCommands)
            {
                if (input.Equals(cmd, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}