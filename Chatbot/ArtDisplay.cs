using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Chatbot
{
    public static class ArtDisplay
    {
        public static void ShowAsciiTitle(MainWindow window)
        {
            string[] welcome = {
                " __        __   _                            _          ",
                " \\ \\      / /__| | ___ ___  _ __ ___   ___  | |_ ___    ",
                "  \\ \\ /\\ / / _ \\ |/ __/ _ \\| '_  _ \\ / _ \\ | __/ _ \\   ",
                "   \\ V  V /  __/ | (_| (_) | | | | | |  __/ | || (_) |  ",
                "    \\_/\\_/ \\___|_|\\___\\___/|_| |_| |_|\\___|  \\__\\___/   ",
            };

            string[] cybersecurity = {
                "",
                "   _____      _                                        _ _         ",
                "  / ____|    | |                                      (_) |        ",
                " | |    _   _| |__   ___ _ __ ___  ___  ___ _   _ _ __ _| |_ _   _ ",
                " | |   | | | | '_ \\ / _ \\ '__/ __|/ _ \\/ __| | | | '__| | __| | | |",
                " | |___| |_| | |_) |  __/ |  \\__ \\  __/ (__| |_| | |  | | |_| |_| |",
                "  \\_____|\\__, |_.__/ \\___|_|  |___/\\___|\\___|\\__,_|_|  |_|\\__|\\__, |",
                "         __/ |                                                __/ |",
                "        |___/                                                |___/ ",
                "                            A W A R E N E S S   B O T"
            };

            // Display with delay
            foreach (var line in welcome)
            {
                window.AppendToChat(line, Brushes.Magenta);
                Thread.Sleep(150);
            }

            Thread.Sleep(300);

            foreach (var line in cybersecurity)
            {
                window.AppendToChat(line, Brushes.Magenta);
                Thread.Sleep(100);
            }
        }

        public static void ShowTipBox(string tip, MainWindow window)
        {
            window.AppendToChat($" ║  🔒 SECURITY TIP: {tip,-25} ║", Brushes.Green);
        }
    }
}