namespace WikipediaBiographyCreator.Interfaces
{
    public interface ITextSearchable
    {        
        string GetObituaryText(string url, string subjectName);
    }
}
