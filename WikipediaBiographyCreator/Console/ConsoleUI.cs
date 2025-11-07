using WikipediaBiographyCreator.Exceptions;
using WikipediaBiographyCreator.Extensions;
using WikipediaBiographyCreator.Interfaces;

namespace WikipediaBiographyCreator.Console
{
    public class ConsoleUI
    {
        private const int TotalWidth = 43;
        private readonly Dictionary<string, Action> _menuOptions;
        private readonly List<MenuItem> _menuItems;
        private readonly IUIActions _uiActions;
        private readonly IAssemblyService _assemblyService;
        private bool _quit;

        private static class MenuOptions
        {
            public const string CrossRefGuardian = "f";
            public const string CrossRefIndy = "c";
            public const string NYTimesObits = "n";
            public const string GuardianObits = "g";
            public const string IndependentObits = "i";
            public const string TestStuff = "t";
            public const string Quit = "q";
        }

        public ConsoleUI(IUIActions uiActions, IAssemblyService assemblyService)
        {
            _uiActions = uiActions;
            _assemblyService = assemblyService;
            _menuItems =
            [
                new("Find candidate; cross-ref Guardian", MenuOptions.CrossRefGuardian, _uiActions.CrossRefGuardian),
                new("Find candidate; cross-ref Independent", MenuOptions.CrossRefIndy, _uiActions.CrossRefIndependent),
                new("Show NYTimes obituary names", MenuOptions.NYTimesObits, _uiActions.ShowNYTimesObituaries),
                new("Show Guardian obituary names", MenuOptions.GuardianObits, _uiActions.ShowGuardianObituaries),
                new("Show Independent obituary names", MenuOptions.IndependentObits, _uiActions.ShowIndependentObituaries),
                new("Test stuff", MenuOptions.TestStuff, _uiActions.TestStuff),
                new("Quit", MenuOptions.Quit, () => _quit = true)
            ];
            _menuOptions = _menuItems.ToDictionary(m => m.Key, m => m.Action, StringComparer.OrdinalIgnoreCase);
        }

        public void Run()
        {
            try
            {
                ExecuteUserInterface();
            }
            catch (Exception e)
            {
                ConsoleFormatter.WriteError(e.ToString());
            }
            finally
            {
                ConsoleFormatter.ResetColor();
            }
        }

        private void ExecuteUserInterface()
        {
            DrawBanner();

            while (!_quit)
            {
                PrintMenuOptions();

                string option = ConsoleFormatter.GetUserInput();

                if (_menuOptions.TryGetValue(option, out var action))
                {
                    try
                    {
                        action();
                    }
                    catch (AppException ex)
                    {
                        ConsoleFormatter.WriteWarning(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        ConsoleFormatter.WriteError(ConsoleFormatter.UnexpectedError(ex));
                    }
                }
                else
                {
                    ConsoleFormatter.WriteWarning(ConsoleFormatter.InvalidOption(option));
                }
            }
        }

        private void PrintMenuOptions()
        {
            ConsoleFormatter.WriteMenuOption(new string('-', TotalWidth));

            foreach (var item in _menuItems)
            {
                ConsoleFormatter.WriteMenuOption($"- {item.Label} ({item.Key})");
            }

            ConsoleFormatter.WriteMenuOption(new string('-', TotalWidth));
        }

        private void DrawBanner()
        {
            var assemblyName = _assemblyService.GetAssemblyName();
            string appVersion = _assemblyService.GetAssemblyValue("Version", assemblyName);
            string blankLine = new string(' ', TotalWidth);

            ConsoleFormatter.WriteBanner(blankLine + "\r\n" + blankLine);
            ConsoleFormatter.WriteBanner($"{assemblyName.Name}".PadMiddle(TotalWidth, ' '));
            ConsoleFormatter.WriteBanner($"version: {appVersion}".PadMiddle(TotalWidth, ' '));
            ConsoleFormatter.WriteBanner(blankLine + "\r\n" + blankLine);
        }

        private record MenuItem(string Label, string Key, Action Action);
    }
}
