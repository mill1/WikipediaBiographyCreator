using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Services
{
    public class UIActions : IUIActions
    {
        private readonly ISignupService _signupService;

        public UIActions(ISignupService signupService)
        {
            _signupService = signupService;
        }

        public void ListSignups()
        {
            List<Signup> signups = _signupService.Get();

            if (!signups.Any())
            {
                ConsoleFormatter.WriteInfo("No signups found.");
                return;
            }

            var maxLength = signups.Max(s => $"{s.Id}{s.Name}{s.PhoneNumber}{s.PartySize}".Length) + 9;
            var line = new string('*', maxLength);

            ConsoleFormatter.WriteInfo($"Current signups:");
            ConsoleFormatter.WriteInfo(line);

            foreach (var signup in signups)
            {
                ConsoleFormatter.WriteSuccess($"{signup}");
            }

            ConsoleFormatter.WriteInfo(line);
            ConsoleFormatter.WriteInfo($"Number of signups: {signups.Count}");
            ConsoleFormatter.WriteInfo($"Average party size: {signups.Average(s => s.PartySize):#.##}");
            var largestParty = signups.OrderByDescending(s => s.PartySize).First();
            ConsoleFormatter.WriteInfo($"Largest: {largestParty.Name}, party of {largestParty.PartySize}");
        }

        public void TestStuff()
        {
            int partySize = GetValidInteger("Party size:");
            ConsoleFormatter.WriteInfo($"Checking data for party size {partySize}...");

            _signupService.TestStuff();
            ConsoleFormatter.WriteInfo("Stuff has been tested");
        }

        private static int GetValidInteger(string prompt)
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
