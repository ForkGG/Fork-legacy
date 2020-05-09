using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace fork.ViewModel
{
    public class ConsoleViewModel : BaseViewModel
    {
        private static string welcomeMsg =
            "__        __   _                            _          _   _ _ _     _ _ \n" +
            "\\ \\      / /__| | ___ ___  _ __ ___   ___  | |_ ___   | \\ | (_) |__ (_) |_   _ ___ \n" +
            " \\ \\ /\\ / / _ \\ |/ __/ _ \\| '_ ` _ \\ / _ \\ | __/ _ \\  |  \\| | | '_ \\| | | | | / __|\n" +
            "  \\ V  V /  __/ | (_| (_) | | | | | |  __/ | || (_) | | |\\  | | | | | | | |_| \\__ \\\n" +
            "   \\_/\\_/ \\___|_|\\___\\___/|_| |_| |_|\\___|  \\__\\___/  |_| \\_|_|_| |_|_|_|\\__,_|___/\n";

        public string ConsoleInput { get; set; } = string.Empty;

        public ObservableCollection<string> ConsoleOutput { get; set; } =
            new ObservableCollection<string>() {welcomeMsg};

        public void RunCommand()
        {
            ConsoleOutput.Add(ConsoleInput);
            // do your stuff here.
            ConsoleInput = String.Empty;
        }

        public void WriteLine(string line)
        {
            Application.Current?.Dispatcher?.Invoke(() => ConsoleOutput.Add(line));
        }
    }
}