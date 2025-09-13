using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IObituarySubjectService<T>
    {
        List<string> ResolveNameVersions(T obituary);
        Subject Resolve(T obituary);
    }
}