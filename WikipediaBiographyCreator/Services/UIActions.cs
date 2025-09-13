using Microsoft.Extensions.Configuration;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Services
{
    public class UIActions : IUIActions
    {
        private readonly IConfiguration _configuration;
        private readonly INYTimesApiService _nyTimesApiService;
        private readonly ISignupService _signupService;

        public UIActions(
            IConfiguration configuration,
            INYTimesApiService nYTimesApiService,
            ISignupService signupService)
        {
            _configuration = configuration;
            _nyTimesApiService = nYTimesApiService;
            _signupService = signupService;
        }

        public void NYTimesObituaries()
        {
            int year = GetIntegerInput("Year:");
            int monthId = GetIntegerInput("Month id:");
            string apiKey = GetNYTimesApiKey();

            var obits = _nyTimesApiService.ResolveObituariesOfMonth(year, monthId, apiKey);

            ConsoleFormatter.WriteInfo($"{obits.Count} NYTimes obituaries have been resolved:");

            foreach (var obit in obits)
            {
                // Get the shortest version of the name                
                var shortestName = obit.Subject.NameVersions.OrderBy(n => n.Length).First();

                ConsoleFormatter.WriteInfo(shortestName);
            }
        }

        private string GetNYTimesApiKey()
        {
            string apiKey = _configuration["NYTimes:ApiKey"];

            if (apiKey == null || apiKey == "TOSET")
            {
                apiKey = GetStringInput("API key:");
            }

            return apiKey;
        }

        public void ListSignups()
        {
            List<Signup> signups = _signupService.Get();

            ConsoleFormatter.WriteInfo($"Current signups:");

            foreach (var signup in signups)
            {
                ConsoleFormatter.WriteSuccess($"{signup}");
            }

            ConsoleFormatter.WriteInfo($"Number of signups: {signups.Count}");
            ConsoleFormatter.WriteInfo($"Average party size: {signups.Average(s => s.PartySize):#.##}");
            var largestParty = signups.OrderByDescending(s => s.PartySize).First();
            ConsoleFormatter.WriteInfo($"Largest: {largestParty.Name}, party of {largestParty.PartySize}");
        }

        public void TestStuff()
        {
            int partySize = GetIntegerInput("Party size:");
            ConsoleFormatter.WriteInfo($"Checking data for party size {partySize}...");

            _signupService.TestStuff();
            ConsoleFormatter.WriteInfo("Stuff has been tested");
        }

        private static string GetStringInput(string prompt)
        {
            while (true)
            {
                string input = ConsoleFormatter.GetUserInput(prompt);

                if (string.IsNullOrWhiteSpace(input))
                {
                    ConsoleFormatter.WriteWarning("Input cannot be empty. Please try again.");
                    continue;
                }
                return input;
            }
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
