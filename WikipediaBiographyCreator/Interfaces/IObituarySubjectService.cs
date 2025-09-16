using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IObituarySubjectService<T>
    {
        Subject Resolve(T obituary);
    }
}