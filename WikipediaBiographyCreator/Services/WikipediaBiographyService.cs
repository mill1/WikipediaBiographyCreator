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
        private readonly IIndependentApiService _independentApiService;
        private readonly INYTimesApiService _nyTimesApiService;
        private readonly INYTimesObitSubjectService _nyTimesObitSubjectService;
        private readonly IWikipediaApiService _wikipediaApiService;
        private readonly IDisambiguationResolver _disambiguationResolver;

        public WikipediaBiographyService(
            IConfiguration configuration,
            IGuardianApiService guardianApiService,
            IIndependentApiService independentApiService,
            INYTimesApiService nyTimesApiService,
            INYTimesObitSubjectService nyTimesObitSubjectService,
            IWikipediaApiService wikipediaApiService,
            IDisambiguationResolver disambiguationResolver)
        {
            _configuration = configuration;
            _guardianApiService = guardianApiService;
            _independentApiService = independentApiService;
            _nyTimesApiService = nyTimesApiService;
            _nyTimesObitSubjectService = nyTimesObitSubjectService;
            _wikipediaApiService = wikipediaApiService;
            _disambiguationResolver = disambiguationResolver;
        }

        public void CrossReferenceWithNYTimes(int year, int monthId, bool checkGuardian)
        {
            ConsoleFormatter.WriteInfo($"Finding candidates for {year}/{monthId}...");

            var obitsToCheck = new List<Obituary>();

           if (checkGuardian)
                obitsToCheck = _guardianApiService.ResolveObituariesOfMonth(year, monthId);
            else
                obitsToCheck = _independentApiService.ResolveObituariesOfMonth(year, monthId);

            var nyTimesObits = _nyTimesApiService.ResolveObituariesOfMonth(year, monthId);

            ConsoleFormatter.WriteInfo($"Resolved {nyTimesObits.Count} NYTimes obituaries and {obitsToCheck.Count} obits to check.");
            ConsoleFormatter.WriteInfo($"Proceeding to check matching names for existence on Wikipedia...");

            int threshold = GetScoreThresholdSetting();
            int nrOfMatches = 0;

            foreach (var obit in obitsToCheck)
            {
                var ctx = new ObituaryContext(obit, nyTimesObits, year, monthId);

                nrOfMatches += CrossReferenceObituary(ctx, threshold);
            }

            ConsoleFormatter.WriteInfo($"All done processing {year}/{monthId}. Number of evaluated matches: {nrOfMatches}");
        }

        private int CrossReferenceObituary(ObituaryContext ctx, int threshold)
        {
            var nyTimesObitNames = ctx.NYTimesObits.Select(o => o.Subject.NormalizedName).ToList();
            var bestMatch = FuzzySharp.Process.ExtractOne(ctx.ObitToCheck.Subject.NormalizedName, nyTimesObitNames);
            string matchedName = bestMatch.Value;

            if (bestMatch.Score < 85 && bestMatch.Score >= 75)
                ConsoleFormatter.WriteWarning($"Matching score = {bestMatch.Score}: '{ctx.ObitToCheck.Subject.NormalizedName}' - '{matchedName}' (NYTimes). Check manually.");

            /*
                Score = 80: 'Michael Aris' - 'Michael Caine' (NYTimes)
                Score = 82: 'WD Hamilton' - 'William Donald Hamilton' (NYTimes)
                Score = 83: 'Barbosa Lima' - 'Alexandre Barboas Lima' (NYTimes)
                Score = 87: 'E. M. Nathanson' - 'Edwin M. Nathanson' (NYTimes)
                Score = 76: 'Dino Leventis' - 'Constantine Leventis' (NYTimes)   <-- this was an actual match. And missing!                
             */

            if (bestMatch.Score < threshold)
                return 0;

            /*
                False positives:
                May 1994: John G. Lord
                https://www.independent.co.uk/news/people/obituary-lord-johnmackie-1438834.html
                https://www.nytimes.com/1994/05/22/obituaries/john-g-lord-70-author-and-producer.html
                August 1997: John B. Elliott
                https://www.independent.co.uk/news/people/obituary-john-elliot-1246657.html
                https://www.nytimes.com/1997/08/09/arts/john-b-elliott-69-art-collector.html
                July 2020: John R. Lewis
                https://www.theguardian.com/film/2020/jul/14/lewis-john-carlino-obituary
                https://www.nytimes.com/2020/07/17/us/john-lewis-dead.html
             */

            var nytObitContext = ctx.NYTimesObits.First(o => o.Subject.NormalizedName == matchedName);
            ctx.NYTimesSubject = nytObitContext.Subject.Name;

            var nameVersions = _nyTimesObitSubjectService.GetNameVersions(ctx.NYTimesSubject);
            var exists = TryResolveExistsOnWikipedia(ctx, nameVersions, matchedName);

            if (!exists)
            {
                var candidate = CreateCandidate(ctx.ObitToCheck, ctx.NYTimesObits, matchedName);

                if (ctx.NYTimesSubject.Contains("Name cannot be resolved."))
                    ConsoleFormatter.WriteSuccess($"{"Weak candidate"}: {candidate}");
                else
                    ConsoleFormatter.WriteSuccess($"{"Strong candidate"}: {candidate}");
            }
            return 1;
        }

        private bool TryResolveExistsOnWikipedia(ObituaryContext ctx, IEnumerable<string> nameVersions, string matchedName)
        {
            foreach (var version in nameVersions)
            {
                ConsoleFormatter.WriteInfo($"Match found: '{version}'. Checking existence on Wikipedia...");

                var pageTitle = _wikipediaApiService.GetPageTitle(version, out bool disamb);

                if (string.IsNullOrEmpty(pageTitle))
                    // No page found for this name version, try next
                    continue;

                if (!disamb)
                {
                    ConsoleFormatter.WriteInfo($"\tPage exists: {pageTitle}");
                    return true;
                }

                if (_disambiguationResolver.TryResolve(ctx, pageTitle))
                    return true;

                var candidate = CreateCandidate(ctx.ObitToCheck, ctx.NYTimesObits, matchedName);
                ConsoleFormatter.WriteSuccess($"Possible candidate: {candidate}");

                return true;
            }
            return false;
        }

        public static string? FindDisambiguationEntry(string wikiText, int birthYear, int deathYear)
        {
            // Example regex: [[Page title]] ... 1932–2001
            string pattern = $@"\[\[(?<title>[^\]]+)\]\]\s*\({birthYear}(?:-|–|—|‒|−|&ndash;|&mdash;|&minus;){deathYear}\)";


            var match = Regex.Match(wikiText, pattern);
            return match.Success ? match.Groups["title"].Value : null;
        }

        public static string? FindDisambiguationEntry(string wikiText, int deathYear)
        {
            // Match: [[Page title]] ... –2001
            string pattern = @"\[\[(?<title>[^\]]+)\]\]\s*\((?<birthYear>\d{4})(?:-|–|—|‒|−|&ndash;|&mdash;|&minus;)(?<deathYear>\d{4})\)";


            var match = Regex.Match(wikiText, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value; // The page name inside [[...]]
            }

            return null;
        }

        private static Candidate CreateCandidate(Obituary obitToCheck, List<Obituary> nyTimesObits, string subjectName)
        {
            return new Candidate
            {
                Name = subjectName,
                WebUrl = obitToCheck.WebUrl,
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
            public Obituary ObitToCheck { get; }
            public List<Obituary> NYTimesObits { get; }
            public int Year { get; }
            public int MonthId { get; }
            public string NYTimesSubject { get; set; } = string.Empty;

            public ObituaryContext(Obituary obitToCheck, List<Obituary> nyTimesObits, int year, int monthId)
            {
                ObitToCheck = obitToCheck;
                NYTimesObits = nyTimesObits;
                Year = year;
                MonthId = monthId;
            }
        }
    }
}