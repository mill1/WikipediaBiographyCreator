namespace WikipediaBiographyCreator.Interfaces
{
    public interface IIndependentApiService : IApiService, ITextSearchable
    {
        string GetHtml(string url);
    }
}
