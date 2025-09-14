using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;
using WikipediaBiographyCreator.Models.Guardian;

namespace WikipediaBiographyCreator.Services
{
    public class GuardianObituarySubjectService : IGuardianObituarySubjectService
    {
        public Subject Resolve(Result obituary)
        {
            return new Subject
            {
                Name = ResolveSubjectName(obituary),
                NameVersions = ResolveNameVersions(obituary)
            };
        }

        public List<string> ResolveNameVersions(Result obituary)
        {
            // TODO improve obviously
            string name = ResolveSubjectName(obituary);

            return new List<string> { name };
        }

        private string ResolveSubjectName(Result obituary)
        {
            // 1999 - 200?: webTitle = "Wally Cole obituary"
            string subjectName = obituary.webTitle.Replace(" obituary", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

            /*
                'Obituary: John Gotti' -> 'John Gotti'
                 Replace all these variants by string.Empty:
                'Obituary: '
                'Obituary:'
                'Obituary letter: '
                'Obituary letters: '
                'Letter: '
                ...
             */
            int pos = subjectName.IndexOf(':');

            if (pos < 0)
                return subjectName;

            return subjectName.Substring(pos + 1).Trim();
        }
    }
}
