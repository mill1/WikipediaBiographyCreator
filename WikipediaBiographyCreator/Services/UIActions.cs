using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Interfaces;

namespace WikipediaBiographyCreator.Services
{
    public class UIActions : IUIActions
    {
        private readonly IGuardianApiService _guardianApiService;
        private readonly INYTimesApiService _nyTimesApiService;

        public UIActions(IGuardianApiService guardianApiService, INYTimesApiService nyTimesApiService)
        {
            _guardianApiService = guardianApiService;
            _nyTimesApiService = nyTimesApiService;
        }

        // TODO refactor DRY? : no will be combined in a single process
        public void GuardianObituaries()
        {
            int year = GetIntegerInput("Year:");
            int monthId = GetIntegerInput("Month id:");

            var obits = _guardianApiService.ResolveObituariesOfMonth(year, monthId);

            ConsoleFormatter.WriteInfo($"{obits.Count} Guardian obituaries have been resolved:");

            obits = obits.OrderBy(o => o.Subject.NameVersions.OrderBy(n => n.Length).First()).ToList();

            foreach (var obit in obits)
            {
                // Get the shortest version of the name                
                var shortestName = obit.Subject.NameVersions.OrderBy(n => n.Length).First();

                ConsoleFormatter.WriteInfo(shortestName);
            }
        }

        public void NYTimesObituaries()
        {
            int year = GetIntegerInput("Year:");
            int monthId = GetIntegerInput("Month id:");

            var obits = _nyTimesApiService.ResolveObituariesOfMonth(year, monthId);

            ConsoleFormatter.WriteInfo($"{obits.Count} NYTimes obituaries have been resolved:");

            obits = obits.OrderBy(o => o.Subject.NameVersions.OrderBy(n => n.Length).First()).ToList();

            foreach (var obit in obits)
            {
                // Get the shortest version of the name                
                var shortestName = obit.Subject.NameVersions.OrderBy(n => n.Length).First();

                ConsoleFormatter.WriteInfo(shortestName);
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
