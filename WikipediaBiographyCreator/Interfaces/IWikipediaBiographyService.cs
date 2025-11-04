using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IWikipediaBiographyService
    {
        void CrossReferenceWithNYTimes(int year, int monthId, bool guardian);

        Candidate CreateBiography();
    }
}