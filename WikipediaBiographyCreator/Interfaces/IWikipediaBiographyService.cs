using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IWikipediaBiographyService
    {
        List<Biography> FindCandidates(int year, int monthId);

        Biography CreateBiography();
    }
}
