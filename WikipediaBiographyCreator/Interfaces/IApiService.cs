using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IApiService
    {
        List<Obituary> ResolveObituariesOfMonth(int year, int monthId, string apiKey);
    }
}
