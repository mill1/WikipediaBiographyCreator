using WikipediaBiographyCreator.Interfaces;

namespace WikipediaBiographyCreator.Services
{
    public class WikipediaApiService : IWikipediaApiService
    {
        public string GetWikipediaArticleName(string nameVersion)
        {
            // TODO Call Wikipedia API to check if an article with a given name exists

            // Not found
            return string.Empty;
        }
    }
}
