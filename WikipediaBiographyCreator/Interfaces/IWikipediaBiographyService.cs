using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IWikipediaBiographyService
    {
        Candidate FindCandidate(int year, int monthId);

        Candidate CreateBiography();
    }
}
