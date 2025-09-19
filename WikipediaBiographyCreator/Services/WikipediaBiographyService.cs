using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
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

        public void FindCandidates(int year, int monthId)
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
                    var nyTimesSubject = nyTimesObits.Where(o => o.Subject.NormalizedName == bestMatch.Value).Select(o => o.Subject.Name).First();

                    var nameVersions = _nyTimesObituarySubjectService.GetNameVersions(nyTimesSubject);
                    var exists = false;

                    // checked if any of the versions exist as an article on Wikipedia. Break if you found it.
                    foreach (var version in nameVersions)
                    {
                        ConsoleFormatter.WriteDebug($"Checking version {version}");

                        var pageTitle = _wikipediaApiService.GetPageTitle(version, out bool disambiguation);

                        if (pageTitle != string.Empty)
                        {
                            if (disambiguation)
                            {
                                // If we run into a disambiguation page we need to resolve the YoB and YoD. In most
                                // cases these be found in the body text of the Guardian article.
                                var bodyText = _guardianApiService.GetObituaryText(guardianObit.ApiUrl, guardianObit.Subject.Name);
                                var (dateOfBirth, dateOfDeath) = _guardianObituarySubjectService.ResolveDoBAndDoD(bodyText);

                                // Get the page content of the disambiguation page
                                var content = _wikipediaApiService.GetPageContent(pageTitle);

                                if (dateOfBirth != DateOnly.MinValue && dateOfDeath != DateOnly.MinValue)
                                {
                                    var entry = FindDisambiguationEntry(content, dateOfBirth.Year, dateOfDeath.Year);

                                    if (entry != null)
                                    {
                                        exists = true;
                                        ConsoleFormatter.WriteInfo($"Page exists: {entry}");
                                        break;
                                    }
                                }

                                if (dateOfBirth == DateOnly.MinValue && dateOfDeath == DateOnly.MinValue)
                                {
                                    // No date info. Look for an entry in the disamb. page with the parameter year
                                    var entry = FindDisambiguationEntry(content, year);

                                    if (entry == null)
                                    {
                                        // If not found try again with the previous year in case we're checking January
                                        if (monthId == 1)
                                        {
                                            entry = FindDisambiguationEntry(content, --year);

                                            if (entry != null)
                                            {
                                                exists = true;
                                                ConsoleFormatter.WriteInfo($"Page exists: {entry}");
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        exists = true;
                                        ConsoleFormatter.WriteInfo($"Page exists: {entry}");
                                        break;
                                    }
                                }

                                // We couldn't find the name in the disambiguation page; possible candidate!
                                exists = true;
                                var candidate = CreateCandidate(guardianObit, nyTimesObits, bestMatch.Value);
                                ConsoleFormatter.WriteSuccess($"Possible candidate: {candidate}");
                                break;
                            }
                            else
                            {
                                exists = true;
                                ConsoleFormatter.WriteInfo($"Page exists: {pageTitle}");
                                break;
                            }
                        }
                    }

                    if (!exists)
                    {
                        // We could be in business!
                        var candidate = CreateCandidate(guardianObit, nyTimesObits, bestMatch.Value);
                        ConsoleFormatter.WriteSuccess($"Strong candidate: {candidate}");
                    }
                }
            }
        }

        public static string? FindDisambiguationEntry(string wikiText, int birthYear, int deathYear)
        {
            // Example regex: [[Page title]] ... 1932–2001
            string pattern = $@"\[\[([^\]]+)\]\][^\n]*\b{birthYear}–{deathYear}\b";

            var match = Regex.Match(wikiText, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value; // The page name inside [[...]]
            }

            return null;
        }

        public static string? FindDisambiguationEntry(string wikiText, int deathYear)
        {
            // Match: [[Page title]] ... –2001
            string pattern = $@"\[\[([^\]]+)\]\][^\n]*–{deathYear}\b";

            var match = Regex.Match(wikiText, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value; // The page name inside [[...]]
            }

            return null;
        }

        private static Candidate CreateCandidate(Obituary guardianObit, List<Obituary> nyTimesObits, string subjectName)
        {
            return new Candidate
            {
                Name = subjectName,
                WebUrlGuardian = guardianObit.WebUrl,
                WebUrlNYTimes = nyTimesObits.Where(o => o.Subject.NormalizedName == subjectName).Select(o => o.WebUrl).First()
            };
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