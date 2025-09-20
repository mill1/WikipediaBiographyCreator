using static WikipediaBiographyCreator.Services.WikipediaBiographyService;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IDisambiguationResolver
    {
        bool TryResolve(ObituaryContext context, string pageTitle);
    }

}
