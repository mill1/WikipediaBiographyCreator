using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Interfaces;
using static WikipediaBiographyCreator.Services.WikipediaBiographyService;

namespace WikipediaBiographyCreator.Services
{
    public class DisambiguationResolver : IDisambiguationResolver
    {
        private readonly IGuardianApiService _guardianApiService;
        private readonly IGuardianObituarySubjectService _guardianObituarySubjectService;
        private readonly IWikipediaApiService _wikipediaApiService;

        public DisambiguationResolver(
            IGuardianApiService guardianApiService,
            IGuardianObituarySubjectService guardianObituarySubjectService,
            IWikipediaApiService wikipediaApiService)
        {
            _guardianApiService = guardianApiService;
            _guardianObituarySubjectService = guardianObituarySubjectService;
            _wikipediaApiService = wikipediaApiService;
        }

        public bool TryResolve(ObituaryContext ctx, string pageTitle)
        {
            var body = _guardianApiService.GetObituaryText(ctx.Guardian.ApiUrl, ctx.Guardian.Subject.Name);
            var (dob, dod) = _guardianObituarySubjectService.ResolveDoBAndDoD(body);
            var content = _wikipediaApiService.GetPageContent(pageTitle);

            if (dob != DateOnly.MinValue && dod != DateOnly.MinValue)
                return TryMatchDisambEntry(content, dob.Year, dod.Year);

            return TryMatchDisambByYear(content, ctx.Year, ctx.MonthId);
        }

        private bool TryMatchDisambEntry(string content, int yob, int yod)
        {
            var entry = FindDisambiguationEntry(content, yob, yod);

            if (entry == null)
                return false;

            ConsoleFormatter.WriteInfo($"Page exists: {entry}");
            return true;
        }

        private bool TryMatchDisambByYear(string content, int year, int monthId)
        {
            var entry = FindDisambiguationEntry(content, year);

            if (entry == null && monthId == 1)
                entry = FindDisambiguationEntry(content, year - 1);

            if (entry == null)
                return false;

            ConsoleFormatter.WriteInfo($"Page exists: {entry}");
            return true;
        }
    }

}
