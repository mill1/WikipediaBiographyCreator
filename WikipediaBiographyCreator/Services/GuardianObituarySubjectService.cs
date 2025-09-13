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
            int pos = obituary.webTitle.IndexOf(':');

            if (pos < 0)
                return obituary.webTitle;

            return obituary.webTitle.Substring(pos + 1).Trim();
        }
    }
}
