using System.Text.RegularExpressions;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Interfaces;
using static WikipediaBiographyCreator.Services.WikipediaBiographyService;

namespace WikipediaBiographyCreator.Services
{
    public class DisambiguationResolver : IDisambiguationResolver
    {
        private readonly IGuardianApiService _guardianApiService;
        private readonly IGuardianObitSubjectService _guardianObitSubjectService;
        private readonly IIndependentApiService _independentApiService;
        private readonly IIndependentObitSubjectService _indyObitSubjectService;
        private readonly IWikipediaApiService _wikipediaApiService;

        public DisambiguationResolver(
            IGuardianApiService guardianApiService,
            IGuardianObitSubjectService guardianObitSubjectService,
            IIndependentApiService independentApiService,
            IIndependentObitSubjectService indyObitSubjectService,
            IWikipediaApiService wikipediaApiService)
        {
            _guardianApiService = guardianApiService;
            _guardianObitSubjectService = guardianObitSubjectService;
            _independentApiService = independentApiService;
            _indyObitSubjectService = indyObitSubjectService;
            _wikipediaApiService = wikipediaApiService;
        }


        public bool TryResolve(ObituaryContext ctx, string pageTitle)
        {
            ITextSearchable fullTextService = null;
            IDoBDoDResolvable doBDoDResolver = null;

            if (ctx.ObitToCheck.Source == "Guardian")
            {
                fullTextService = _guardianApiService;
                doBDoDResolver = _guardianObitSubjectService;
            }
            else // Independent
            {
                fullTextService = _independentApiService;
                doBDoDResolver = _indyObitSubjectService;
            }

            var body = fullTextService.GetObituaryText(ctx.ObitToCheck.FullTextUrl, ctx.ObitToCheck.Subject.Name);
            var (dob, dod) = doBDoDResolver.ResolveDoBAndDoD(body);

            var content = _wikipediaApiService.GetPageContent(pageTitle);

            // Check redirect to disambiguation page. 
            if (content.Contains("#REDIRECT"))
            {
                // Get the disambiguation page 
                var redirectionTarget = GetRedirectTarget(content);
                content = _wikipediaApiService.GetPageContent(redirectionTarget);
            }

            if (dob != DateOnly.MinValue && dod != DateOnly.MinValue)
                return TryMatchDisambiguationEntry(content, dob.Year, dod.Year);

            return TryMatchDisambiguationByYear(content, ctx.Year, ctx.MonthId);
        }

        private bool TryMatchDisambiguationEntry(string content, int yob, int yod)
        {
            var entry = FindDisambiguationEntry(content, yob, yod);

            if (entry == null)
                return false;

            ConsoleFormatter.WriteInfo($"\tPage exists: {entry}");
            return true;
        }

        private bool TryMatchDisambiguationByYear(string content, int year, int monthId)
        {
            var entry = FindDisambiguationEntry(content, year);

            if (entry == null && monthId == 1)
                entry = FindDisambiguationEntry(content, year - 1);

            if (entry == null)
                return false;

            ConsoleFormatter.WriteInfo($"\tPage exists: {entry}");
            return true;
        }

        private string GetRedirectTarget(string text)
        {
            var match = Regex.Match(text, @"#REDIRECT\s+\[\[(.*?)\]\]", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            throw new Exception("Redirection target could not be resolved."); // Tommy_Devito: #REDIRECT[[Tommy DeVito]]
        }
    }
}
