using Newtonsoft.Json;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Exceptions;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models.Independent;

namespace WikipediaBiographyCreator.Services
{
    public class UIActions : IUIActions
    {
        private readonly IGuardianApiService _guardianApiService;
        private readonly INYTimesApiService _nyTimesApiService;
        private readonly IIndependentApiService _independentApiService;
        private readonly IWikipediaBiographyService _wikipediaBiographyService;

        public UIActions(
            IGuardianApiService guardianApiService,
            INYTimesApiService nyTimesApiService,
            IIndependentApiService independentApiService,
            IWikipediaBiographyService wikipediaBiographyService)
        {
            _guardianApiService = guardianApiService;
            _nyTimesApiService = nyTimesApiService;
            _independentApiService = independentApiService;
            _wikipediaBiographyService = wikipediaBiographyService;
        }

        public void FindCandidate()
        {
            int year = GetIntegerInput("Year:");

            if (year < 1999)
            {
                ConsoleFormatter.WriteWarning("Year must be 1999 or later.");
                return;
            }

            int monthId = GetIntegerInput("Month id:");

            if (monthId < 1 || monthId > 13)
            {
                ConsoleFormatter.WriteWarning("Month id must be between 1 and 12.");
                return;
            }

            _wikipediaBiographyService.FindCandidates(year, monthId);
        }

        public void ShowGuardianObituaries()
        {
            ShowObituaryNames(_guardianApiService, "Guardian");
        }

        public void ShowNYTimesObituaries()
        {
            ShowObituaryNames(_nyTimesApiService, "NYTimes");
        }

        public void ShowIndependentObituaries()
        {
            ShowObituaryNames(_independentApiService, "Independent");
        }

        private void ShowObituaryNames(IApiService apiService, string sourceName)
        {
            int year = GetIntegerInput("Year:");

            if(sourceName == "Guardian")
                if (year < 1999)
                    throw new ArgumentException("Year must be 1999 or later.");

            if (sourceName == "Independent")
                if (year < 1992)
                    throw new ArgumentException("Year must be 1992 or later.");

            int monthId = GetIntegerInput("Month id:");

            if (sourceName == "Independent")
                if (year == 1992 && monthId < 7)
                    throw new ArgumentException("Month  must be July 1992 or later.");

            var obits = apiService.ResolveObituariesOfMonth(year, monthId).OrderBy(o => o.Subject.Name);

            ConsoleFormatter.WriteInfo($"{obits.Count()} {sourceName} obituaries have been resolved:");

            foreach (var obit in obits)
            {
                ConsoleFormatter.WriteInfo($"{obit.Subject.Name}");
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
