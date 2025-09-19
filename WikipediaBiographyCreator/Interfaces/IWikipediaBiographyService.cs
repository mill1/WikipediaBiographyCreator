using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IWikipediaBiographyService
    {
        void FindCandidates(int year, int monthId);

        Candidate CreateBiography();
    }
}