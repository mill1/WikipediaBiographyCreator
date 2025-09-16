using Microsoft.Extensions.Configuration;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Services
{
    public class WikipediaBiographyService : IWikipediaBiographyService
    {
        private readonly IConfiguration _configuration;
        private readonly IGuardianApiService _guardianApiService;
        private readonly INYTimesApiService _nyTimesApiService;
        private readonly IGuardianObituarySubjectService _guardianObituarySubjectService;

        public WikipediaBiographyService(
            IConfiguration configuration,
            IGuardianApiService guardianApiService,
            INYTimesApiService nyTimesApiService,
            IGuardianObituarySubjectService guardianObituarySubjectService)
        {
            _configuration = configuration;
            _guardianApiService = guardianApiService;
            _nyTimesApiService = nyTimesApiService;
            _guardianObituarySubjectService = guardianObituarySubjectService;
        }

        public List<Biography> FindCandidates(int year, int monthId)
        {
            var guardianObits = _guardianApiService.ResolveObituariesOfMonth(year, monthId);
            var nyTimesObits = _nyTimesApiService.ResolveObituariesOfMonth(year, monthId);
            var nyTimesObitNames = nyTimesObits.Select(o => o.Subject.NormalizedName).ToList();

            ConsoleFormatter.WriteDebug("Subject Guardian -> Subject NYTimes (score)");

            foreach (Obituary guardianObit in guardianObits)
            {
                // Find the best match in obitNamesNYTimes for each name in obitNamesGuardian
                var bestMatch = FuzzySharp.Process.ExtractOne(guardianObit.Subject.NormalizedName, nyTimesObitNames);
                int scoreThreshold = GetScoreThresholdSetting();

                if (bestMatch.Score >= scoreThreshold)
                {
                    ConsoleFormatter.WriteDebug($"{guardianObit} -> {bestMatch.Value} (score: {bestMatch.Score})");

                    // For each match:
                    // - Get the body text of the Guardian obituary
                    // - Check if a corresponding Wikipedia article exists
                    // - If not, create a new Wikipedia article draft

                    var bodyText = _guardianApiService.GetObituaryText(guardianObit.ApiUrl, guardianObit.Subject.Name);

                    // Check for existing Wikipedia article
                    // We need to resolve the YoB and YoD first in case of disambiguation pages
                    var (YearOfBirth, YearOfDeath) = _guardianObituarySubjectService.ResolveYoBAndYoD(bodyText);

                    // Determine the name versions for which we need to check Wikipedia
                    // TODO: resolve the first and last name from the NYTimes obit data

                }
            }

            return new List<Biography>();
        }

        public Biography CreateBiography()
        {
            throw new NotImplementedException();
        }

        private int GetScoreThresholdSetting()
        {
            string ScoreThresholdSetting = _configuration["Fuzzy search:Score threshold"];

            // Try to parse the setting, default to 85 if parsing fails
            if (!int.TryParse(ScoreThresholdSetting, out int scoreThreshold))
            {
                ConsoleFormatter.WriteWarning("Invalid score threshold setting. See README, defaulting to 85.");
                scoreThreshold = 85;
            }

            return scoreThreshold;
        }
    }
}
