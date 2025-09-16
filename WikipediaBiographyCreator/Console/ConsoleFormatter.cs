namespace WikipediaBiographyCreator.Console
{
    public static class ConsoleFormatter
    {
        private static void WriteLine(string message, ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black)
        {
            System.Console.ForegroundColor = foreground;
            System.Console.BackgroundColor = background;

            System.Console.WriteLine(message);
        }

        // Output helpers
        public static void WriteError(string message) => WriteLine($"{message}", ConsoleColor.Red);
        public static void WriteWarning(string message) => WriteLine($"{message}", ConsoleColor.Magenta);
        public static void WriteInfo(string message) => WriteLine($"{message}", ConsoleColor.Cyan);
        public static void WriteSuccess(string message) => WriteLine($"{message}", ConsoleColor.Green);
        public static void WriteMenuOption(string message) => WriteLine($"{message}", ConsoleColor.Yellow);
        public static void WriteBanner(string message) => WriteLine($"{message}", ConsoleColor.White, ConsoleColor.DarkMagenta);

        public static string GetUserInput(string prompt = "")
        {
            if (!string.IsNullOrWhiteSpace(prompt))
                WriteInfo(prompt);

            System.Console.ForegroundColor = ConsoleColor.White;
            return System.Console.ReadLine()?.Trim() ?? string.Empty;
        }

        public static void ResetColor()
        {
            System.Console.ResetColor();
        }

        #region Error message builders
        public static string UnexpectedError(Exception ex) =>
           $"Unexpected error: {ex.Message}";

        public static string InvalidOption(string option) =>
            $"Invalid option: {option}";
        #endregion
    }
}
