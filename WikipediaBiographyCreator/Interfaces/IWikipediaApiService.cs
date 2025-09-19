namespace WikipediaBiographyCreator.Interfaces
{
    public interface IWikipediaApiService
    {
        string GetPageTitle(string nameVersion, out bool disambiguation);
        string GetPageContent(string nameVersion);
    }
}