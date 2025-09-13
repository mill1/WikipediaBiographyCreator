using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface INYTimesObituarySubjectService : IObituarySubjectService
    {
        Subject Resolve(Doc doc);
    }
}