using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface INYTimesObituarySubjectService
    {
        Subject Resolve(Doc doc);
        List<string> ResolveNameVersions(Doc doc);
    }
}