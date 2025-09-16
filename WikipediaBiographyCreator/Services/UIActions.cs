using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Interfaces;

namespace WikipediaBiographyCreator.Services
{
    public class UIActions : IUIActions
    {
        private readonly IGuardianApiService _guardianApiService;
        private readonly INYTimesApiService _nyTimesApiService;
        private readonly IWikipediaBiographyService _wikipediaBiographyService;

        public UIActions(
            IGuardianApiService guardianApiService,
            INYTimesApiService nyTimesApiService,
            IWikipediaBiographyService wikipediaBiographyService)
        {
            _guardianApiService = guardianApiService;
            _nyTimesApiService = nyTimesApiService;
            _wikipediaBiographyService = wikipediaBiographyService;
        }

        public void FindCandidates()
        {
            int year = GetIntegerInput("Year:");
            int monthId = GetIntegerInput("Month id:");

            ConsoleFormatter.WriteInfo($"Finding candidates for {year}-{monthId}...");

            var bios = _wikipediaBiographyService.FindCandidates(year, monthId);

            ConsoleFormatter.WriteInfo($"{bios.Count} candidates have been found.");
        }

        public void ShowGuardianObituaries()
        {
            ShowObituaries(_guardianApiService, "Guardian");
        }

        public void ShowNYTimesObituaries()
        {
            ShowObituaries(_nyTimesApiService, "NYTimes");
        }

        private void ShowObituaries(IApiService apiService, string sourceName)
        {
            int year = GetIntegerInput("Year:");
            int monthId = GetIntegerInput("Month id:");

            var obits = apiService.ResolveObituariesOfMonth(year, monthId);

            ConsoleFormatter.WriteInfo($"{obits.Count} {sourceName} obituaries have been resolved:");

            foreach (var obit in obits)
            {
                ConsoleFormatter.WriteInfo($"{obit.Subject.NormalizedName}"); // ({obit.Subject.Name})");
            }
        }

        public void TestStuff()
        {
            int partySize = GetIntegerInput("Party size:");
            ConsoleFormatter.WriteInfo($"Checking data for party size {partySize}...");
            Thread.Sleep(2000);
            ConsoleFormatter.WriteInfo("Stuff has been tested");
        }

        private static int GetIntegerInput(string prompt)
        {
            while (true)
            {
                string input = ConsoleFormatter.GetUserInput(prompt);

                if (string.IsNullOrWhiteSpace(input))
                {
                    ConsoleFormatter.WriteWarning("Input cannot be empty. Please try again.");
                    continue;
                }

                if (int.TryParse(input, out int result))
                    return result;

                ConsoleFormatter.WriteWarning("Invalid number. Please try again.");
            }
        }
    }
}
