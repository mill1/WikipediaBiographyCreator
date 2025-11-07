using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IIndependentObitSubjectService : IObitSubjectService<Obituary>, IDoBDoDResolvable
    {
    }
}
