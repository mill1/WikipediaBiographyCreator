namespace WikipediaBiographyCreator.Interfaces
{
    public interface INameVersionService
    {
        List<string> GetNameVersions(string firstnames, string surname, string suffix);
    }
}
