using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IObitSubjectService<T>
    {
        Subject Resolve(T obituary);
    }
}