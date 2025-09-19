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
        private readonly INYTimesObituarySubjectService _nyTimesObituarySubjectService;
        private readonly IWikipediaApiService _wikipediaApiService;

        public WikipediaBiographyService(
            IConfiguration configuration,
            IGuardianApiService guardianApiService,
            INYTimesApiService nyTimesApiService,
            IGuardianObituarySubjectService guardianObituarySubjectService,
            INYTimesObituarySubjectService nyTimesObituarySubjectService,
            IWikipediaApiService wikipediaApiService)
        {
            _configuration = configuration;
            _guardianApiService = guardianApiService;
            _nyTimesApiService = nyTimesApiService;
            _guardianObituarySubjectService = guardianObituarySubjectService;
            _nyTimesObituarySubjectService = nyTimesObituarySubjectService;
            _wikipediaApiService = wikipediaApiService;
        }

        public Candidate FindCandidate(int year, int monthId)
        {
            var guardianObits = _guardianApiService.ResolveObituariesOfMonth(year, monthId);
            var nyTimesObits = _nyTimesApiService.ResolveObituariesOfMonth(year, monthId);
            var nyTimesObitNames = nyTimesObits.Select(o => o.Subject.NormalizedName).ToList();

            foreach (Obituary guardianObit in guardianObits)
            {
                // Find the best match in obitNamesNYTimes for each name in obitNamesGuardian
                var bestMatch = FuzzySharp.Process.ExtractOne(guardianObit.Subject.NormalizedName, nyTimesObitNames);
                int scoreThreshold = GetScoreThresholdSetting();

                if (bestMatch.Score >= scoreThreshold)
                {
                    //ConsoleFormatter.WriteDebug($"{guardianObit} -> {bestMatch.Value} (score: {bestMatch.Score})");
                    //ConsoleFormatter.WriteDebug(guardianObit.WebUrl);

                    // Determine the name versions for which we need to check Wikipedia
                    var nameVersions = _nyTimesObituarySubjectService.GetNameVersions(bestMatch.Value);
                    var exists = false;

                    // checked if any of the versions exist as an article on Wikipedia
                    foreach (var version in nameVersions)
                    {
                        var wikipediaPageTitle = _wikipediaApiService.GetWikipediaPageTitle(version);

                        if (wikipediaPageTitle != string.Empty)
                        {
                            exists = true;
                            ConsoleFormatter.WriteInfo($"Page exists: {wikipediaPageTitle}");
                            break;
                        }
                    }

                    if (!exists)
                    {
                        // We are in business!
                        return new Candidate
                        {
                            Name = bestMatch.Value,
                            WebUrlGuardian = guardianObit.WebUrl,
                            WebUrlNYTimes = nyTimesObits.Where(o => o.Subject.NormalizedName == bestMatch.Value).Select(o => o.WebUrl).First()
                        };
                    }


                    // If we run into a disambiguation pages we need to resolve the YoB and YoD. In most
                    // cases these be found in the body text of the Guardian article.

                    //var bodyText = _guardianApiService.GetObituaryText(guardianObit.ApiUrl, guardianObit.Subject.Name);
                    //var (yearOfBirth, yearOfDeath) = _guardianObituarySubjectService.ResolveYoBAndYoD(bodyText);

                    //if (yearOfDeath == -1)
                    //    yearOfDeath = year; // Use the obituary year as YoD if we cannot resolve it from the text                        

                }
            }

            // No candidates found :(
            return null;
        }

        public Candidate CreateBiography()
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