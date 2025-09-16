namespace WikipediaBiographyCreator.Interfaces
{
    public interface IGuardianApiService : IApiService
    {
        string GetObituaryText(string apiUrl, string subjectName);
    }
}
