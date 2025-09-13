using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IObituarySubjectService
    {
        List<string> ResolveNameVersions(Doc doc);
    }
}