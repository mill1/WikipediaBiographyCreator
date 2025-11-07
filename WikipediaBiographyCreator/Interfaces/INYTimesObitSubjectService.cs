using WikipediaBiographyCreator.Models.NYTimes;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface INYTimesObitSubjectService : IObitSubjectService<Doc>
    {
        public List<string> GetNameVersions(string subjectName);
    }
}