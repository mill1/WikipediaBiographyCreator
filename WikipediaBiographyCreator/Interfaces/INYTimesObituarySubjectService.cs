using WikipediaBiographyCreator.Models.NYTimes;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface INYTimesObituarySubjectService : IObituarySubjectService<Doc>
    {
        public List<string> GetNameVersions(string subjectName);
    }
}