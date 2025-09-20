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
        private readonly INYTimesObituarySubjectService _nyTimesObituarySubjectService;
        private readonly IWikipediaApiService _wikipediaApiService;
        private readonly IDisambiguationResolver _disambiguationResolver;

        public WikipediaBiographyService(
            IConfiguration configuration,
            IGuardianApiService guardianApiService,
            INYTimesApiService nyTimesApiService,
            INYTimesObituarySubjectService nyTimesObituarySubjectService,
            IWikipediaApiService wikipediaApiService,
            IDisambiguationResolver disambiguationResolver)
        {
            _configuration = configuration;
            _guardianApiService = guardianApiService;
            _nyTimesApiService = nyTimesApiService;
            _nyTimesObituarySubjectService = nyTimesObituarySubjectService;
            _wikipediaApiService = wikipediaApiService;
            _disambiguationResolver = disambiguationResolver;
        }

        public void FindCandidates(int year, int monthId)
        {
            var guardianObits = _guardianApiService.ResolveObituariesOfMonth(year, monthId);
            var nyTimesObits = _nyTimesApiService.ResolveObituariesOfMonth(year, monthId);

            int threshold = GetScoreThresholdSetting();

            foreach (var guardianObit in guardianObits)
            {
                var ctx = new ObituaryContext(guardianObit, nyTimesObits, year, monthId);
                ProcessGuardianObituary(ctx, threshold);
            }
        }

        private void ProcessGuardianObituary(ObituaryContext ctx, int threshold)
        {
            var nyTimesObitNames = ctx.NyTimesObits.Select(o => o.Subject.NormalizedName).ToList();
            var bestMatch = FuzzySharp.Process.ExtractOne(ctx.Guardian.Subject.NormalizedName, nyTimesObitNames);
            string matchedName = bestMatch.Value;

            if (bestMatch.Score < 85 && bestMatch.Score >= 80)
                ConsoleFormatter.WriteWarning($"Score = {bestMatch.Score}: '{ctx.Guardian.Subject.NormalizedName}' - '{matchedName}' (NYTimes). Check manually");

            /*
             * 4:
                Score = 80: 'Michael Aris' - 'Michael Caine' (NYTimes)
                Score = 80: 'Nana Opoku Ware II' - 'Otumfuo Opoku Ware Ii' (NYTimes)
                Score = 86: 'Abdul Aziz Ibn Baz' - 'Abdelaziz Bin Baz' (NYTimes)
             */

            if (bestMatch.Score < threshold)
                return;

            ctx.NyTimesSubject = ctx.NyTimesObits.First(o => o.Subject.NormalizedName == matchedName).Subject.Name;

            var nameVersions = _nyTimesObituarySubjectService.GetNameVersions(ctx.NyTimesSubject);
            var exists = TryResolveWikipediaExistence(ctx, nameVersions, matchedName);

            if (!exists)
            {
                var candidate = CreateCandidate(ctx.Guardian, ctx.NyTimesObits, matchedName);
                var msg = ctx.NyTimesSubject.Contains("Name cannot be resolved.")
                    ? "Weak candidate" : "Strong candidate";

                ConsoleFormatter.WriteSuccess($"{msg}: {candidate}");
            }
        }

        private bool TryResolveWikipediaExistence(ObituaryContext ctx, IEnumerable<string> nameVersions, string matchedName)
        {
            foreach (var version in nameVersions)
            {
                ConsoleFormatter.WriteInfo($"\tChecking name '{version}'...");
                var pageTitle = _wikipediaApiService.GetPageTitle(version, out bool disamb);

                if (string.IsNullOrEmpty(pageTitle))
                    continue;

                if (!disamb)
                {
                    ConsoleFormatter.WriteInfo($"Page exists: {pageTitle}");
                    return true;
                }

                if (_disambiguationResolver.TryResolve(ctx, pageTitle))
                    return true;

                var candidate = CreateCandidate(ctx.Guardian, ctx.NyTimesObits, matchedName);
                ConsoleFormatter.WriteSuccess($"Possible candidate: {candidate}");

                return true;
            }
            return false;
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

        public class ObituaryContext
        {
            public Obituary Guardian { get; }
            public List<Obituary> NyTimesObits { get; }
            public int Year { get; }
            public int MonthId { get; }
            //   public string MatchValue { get; set; } = string.Empty;
            public string NyTimesSubject { get; set; } = string.Empty;

            public ObituaryContext(Obituary guardian, List<Obituary> nyTimesObits, int year, int monthId)
            {
                Guardian = guardian;
                NyTimesObits = nyTimesObits;
                Year = year;
                MonthId = monthId;
            }
        }
    }
}