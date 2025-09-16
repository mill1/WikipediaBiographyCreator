using System.Text.RegularExpressions;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;
using WikipediaBiographyCreator.Models.Guardian;

namespace WikipediaBiographyCreator.Services
{
    public class GuardianObituarySubjectService : IGuardianObituarySubjectService
    {
        public Subject Resolve(Result obituary)
        {
            string subjectName = ResolveSubjectName(obituary);

            return new Subject
            {
                Name = subjectName,
                NormalizedName = GetNormalizedName(subjectName),
            };
        }
        public (int YearOfBirth, int YearOfDeath) ResolveYoBAndYoD(string obituaryText)
        {
            // TODO implement
            return (-1, -1);
        }

        private string ResolveSubjectName(Result obituary)
        {
            string subjectName = obituary.webTitle;

            /*
                'Obituary: John Gotti' -> 'John Gotti'
                 Replace all these variants by string.Empty:
                'Obituary: '
                'Obituary:'
                'Obituary letter: '
                'Obituary letters: '
                'Letter: '
                'Letters: '
                'Letters: the late Alan Ross'
             */
            int pos = subjectName.IndexOf(':');

            if (pos >= 0)
                subjectName = subjectName.Substring(pos + 1).Trim();

            //  'Obituaries: Letter: Len Shackleton'
            //  'Obituaries: Letters: Len Shackleton'
            pos = subjectName.IndexOf(':');

            if (pos >= 0)
                subjectName = subjectName.Substring(pos + 1).Trim();

            return SanitizeSubjectName(subjectName);
        }

        private static string GetNormalizedName(string subjectName)
        {
            // Replace all single-character words (A-Z) with "X." if not at the end of the string.
            var GetNormalizedName = Regex.Replace(
                subjectName,
                @"\b([A-Z])\b(?!$)",
                "$1."
            );

            return GetNormalizedName;
        }

        private static string SanitizeSubjectName(string subjectName)
        {
            // 1999 - 200?: webTitle = "Wally Cole obituary" -> "Wally Cole"
            var name = subjectName.Replace(" obituary", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

            // More sanitization; remove precedings titles like 'Sir', 'Dame', et cetera. Add a space after each title to prevent removing parts of names.
            var titles = new List<string>
            {
                "Sir ",
                "Dame ",
                "Lord ",
                "Lady ",
                "Count",
                "Countess ",
                "Baron ",
                "Baroness ",
                "Capt ",
                "Captain ",
                "Admiral ",
                "Admiral of the Fleet",
                "Rear-Admiral ",
                "Major ",
                "Colonel ",
                "Gen ",
                "General ",
                "Lt ",
                "Lieutenant ",
                "Dr ",
                "Doctor ",
                "Rev ",
                "The Rev ",
                "Reverend ",
                "Fr ",
                "Father ",
                "Rabbi ",
                "Bishop ",
                "Archbishop ",
                "Cardinal ",
                "President ",
                "Prime Minister ",
                "PM ",
                "Chancellor ",
                "Senator ",
                "Ambassador ",
                "Minister ",
                "Governor ",
                "Mayor ",
                "Judge ",
                "Lord ",
                "Prof ",
                "Professor ",
                "the late ",
                "The late ",
                "Italian photographer ",
                "Obituary "
            };

            foreach (var title in titles)
            {
                if (name.StartsWith(title))
                {
                    name = name.Substring(title.Length).Trim();
                    break; // only remove one leading title
                }
            }

            return name.Trim();
        }
    }
}
