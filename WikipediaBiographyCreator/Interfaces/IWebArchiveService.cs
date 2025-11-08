namespace WikipediaBiographyCreator.Interfaces
{
    public interface IWebArchiveService
    {
        IEnumerable<string> ResolveUrlsTheIndependent();
    }
}
