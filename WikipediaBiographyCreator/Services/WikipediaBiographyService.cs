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

            //TMP: For testing purposes only
            List<string> nyTimesObitNames = null;

            if (true)
                nyTimesObitNames = GetNYTimesDataJan1999();
            else
            {
                var nyTimesObits = _nyTimesApiService.ResolveObituariesOfMonth(year, monthId);
                nyTimesObitNames = nyTimesObits.Select(o => o.Subject.NormalizedName).ToList();
            }

            ConsoleFormatter.WriteDebug("Subject Guardian -> Subject NYTimes (score)");

            foreach (Obituary guardianObit in guardianObits)
            {
                // Find the best match in obitNamesNYTimes for each name in obitNamesGuardian
                var bestMatch = FuzzySharp.Process.ExtractOne(guardianObit.Subject.NormalizedName, nyTimesObitNames);
                int scoreThreshold = GetScoreThresholdSetting();

                if (bestMatch.Score >= 0) // scoreThreshold)
                {
                    // ConsoleFormatter.WriteDebug($"{guardianObit} -> {bestMatch.Value} (score: {bestMatch.Score})");

                    // For each match:
                    // - Get the body text of the Guardian obituary
                    // - Check if a corresponding Wikipedia article exists
                    // - If not, create a new Wikipedia article draft

                    var bodyText = _guardianApiService.GetObituaryText(guardianObit.ApiUrl, guardianObit.Subject.Name);

                    // Check for existing Wikipedia article
                    // We need to resolve the YoB AND YoD first in case of disambiguation pages

                    ConsoleFormatter.WriteDebug(guardianObit.WebUrl);

                    // Display the 60 characters after the last occurence of string ' died ' in the body text for debugging purposes.
                    int pos = bodyText.LastIndexOf(" died ");
                    if (pos >= 0)
                    {
                        int start = pos + " died ".Length;
                        int length = Math.Min(60, bodyText.Length - start);
                        string snippet = bodyText.Substring(start, length);
                        ConsoleFormatter.WriteDebug($"...died {snippet}...");
                    }
                    else
                    {
                        ConsoleFormatter.WriteDebug("The word ' died ' was not found in the obituary text.");
                    }

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

        private List<string> GetNYTimesDataJan1999()
        {
            return new List<string>
            {
                "Abbie Hoffman",
                "Abdel-latif Baghdadi",
                "Arthur C. Fatt",
                "August Everding",
                "Bennett Harrison",
                "Bessie Delany",
                "Betty Bryant",
                "Brian Moore",
                "Bryn Jones",
                "Buzz Kulik",
                "Carl Elliott",
                "Charles Brown",
                "Charles Earl Cobb",
                "Charles Francis Adams",
                "Charles G. Zubrod",
                "Charles Luckman",
                "Clare Potter",
                "Damon Wright",
                "David C. Adams",
                "David Dempsey",
                "David Manners",
                "David Newton",
                "David W. Belin",
                "Don Taylor",
                "Edgar Nollner",
                "Edward Joseph Kiernan",
                "Edward M. Mervosh",
                "Edward T. Parrack",
                "Eric Crull Baade",
                "Ernest Schier",
                "Eugene S. Pulliam",
                "Fabrizio De Andre",
                "Florendo M. Visitacion",
                "Frances Gershwin Godowsky",
                "Fred Hopkins",
                "Frederick Zissu",
                "Gabor Carelli",
                "Gavin W. H. Relly",
                "Gayle Young",
                "George Jackson Eder",
                "George L. Mosse",
                "Gilbert M. Haggerty",
                "Gonzalo Torrente Ballester",
                "Goro Yamaguchi",
                "Hanna F. Sulner",
                "Harold Edelman",
                "Harold P. Spivak",
                "Harvey Miller",
                "Henry Cohen",
                "Henry Paolucci",
                "Henry Schwartz",
                "Iron Eyes Cody",
                "Jacques Lecoq",
                "James Hammersetein",
                "James Holmes",
                "James Priest Gifford",
                "Jane Clapperton Cushman",
                "Jay Pritzker",
                "Jerry Quarry",
                "Jerzy Grotowski",
                "Joan Engel Stern",
                "John D. Mcdonald",
                "John Frederick Nims",
                "Jose Vela Zanetti",
                "Judith S. Kestenberg",
                "Jules W. Lederer",
                "Katherine Bain",
                "Leo Cherne",
                "Leon M. Goldstein",
                "Leonard C. Lewin",
                "Lewis J. Gorin Jr.",
                "Linwood P. Shipley",
                "Lorin E. Price",
                "Louis Jolyon West",
                "Lucille Kallen",
                "Manfred L. Karnovsky",
                "Margaret Wentworth Owings",
                "Mario Dario Grossi",
                "Mark Warren",
                "Marshall Perlin",
                "Mary Ann Unger",
                "Merle E. Frampton",
                "Michael Petricciani",
                "Miriam Freund-rosenthal",
                "Monroe W. Karmin",
                "Myles Tierney",
                "Name Cannot Be Resolved. Main: William A. Lee Is De",
                "Naomi Mitchison",
                "Natale Laurendi",
                "Ntsu Mokhehle",
                "Orlandus Wilson",
                "Paul Corser",
                "Paul E. Manheim",
                "Paul M. Zoll",
                "Paul Metcalf",
                "Philip Sterling",
                "Pope",
                "Richard Harold Freyberg",
                "Rita V. Tishman",
                "Robert Douglas",
                "Robert E. Kirby",
                "Robert S. Johnson",
                "Robert Shaw",
                "Rolf Liebermann",
                "Ruth Rawlings Mott",
                "Sammy Solovitz",
                "Susan Strasberg",
                "Ted Hustead",
                "Terence Thornton Lewin",
                "Theo Hios",
                "Theodore Tannenwald Jr.",
                "Thomas C. Mann",
                "Thomas W. Binford",
                "VERNON Berg III",
                "Virginia Eloise Peterson Abelson",
                "Virginia Verrill",
                "W. Page Keeton",
                "Walker Hancock",
                "Walter Donald Kring",
                "Walter H. Page",
                "William Bentley Ball",
                "William E. Hunt",
                "William H. Whyte",
                "William Milfred Batten",
                "Zalman Chaim Bernstein"
            };
        }
    }
}