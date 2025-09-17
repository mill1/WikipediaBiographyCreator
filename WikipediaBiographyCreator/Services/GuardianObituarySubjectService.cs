using System.Globalization;
using System.Text.RegularExpressions;
using WikipediaBiographyCreator.Console;
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
            int yearOfBirth = -1;
            int yearOfDeath = -1;

            int pos = obituaryText.LastIndexOf(" born ");
            if (pos >= 0)
            {
                // Get the 60 characters after the last occurence of word ' born ' in the obituary text.
                int start = pos + " born ".Length;
                int length = Math.Min(60, obituaryText.Length - start);
                string snippet = obituaryText.Substring(start, length);

                /*
                    Many Guardian obituary texts end stating the DoB and DoD in this exact format:
                    [Name], [profession], born [DoB]; died [DoD]
                    e.g.
                    Giorgio Armani, fashion designer, born 11 July 1934; died 4 September 2025

                    Examples of snippets found:
                    Start date data:  September 13, 1922; died January 21, 1999...
                    Start date data:  on an Oklahoma farm. His mother was a Cree, his father a Che...
                    Start date data:  November 1, 1897; died January 11, 1999...
                    Start date data:  October 1, 1917; died January 12, 1999 Godfrey Hodgson...
                */

                if (!snippet.Contains(" died "))
                    return (yearOfBirth, yearOfDeath);

                var regex = new Regex(
                    @"(?<dob>(?:[A-Za-z]+\s+\d{1,2},\s*\d{4}|\d{1,2}\s+[A-Za-z]+\s+\d{4}))\s*;\s*died\s+(?<dod>(?:[A-Za-z]+\s+\d{1,2},\s*\d{4}|\d{1,2}\s+[A-Za-z]+\s+\d{4}))",
                    RegexOptions.IgnoreCase);

                var match = regex.Match(snippet);
                if (match.Success)
                {
                    if (DateTime.TryParse(match.Groups["dob"].Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob))
                        yearOfBirth = dob.Year;

                    if (DateTime.TryParse(match.Groups["dod"].Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dod))
                        yearOfDeath = dod.Year;
                }
            }
            ConsoleFormatter.WriteDebug($"Resolved YoB: {yearOfBirth}, YoD: {yearOfDeath}");

            return (yearOfBirth, yearOfDeath);
        }


        public (int YearOfBirth, int YearOfDeath) ResolveYoBAndYoD2(string obituaryText)
        {
            // YoB = Year of Birth, YoD = Year of Death, DoB = Date of Birth, DoD = Date of Death
            int yearOfBirth = -1;
            int yearOfDeath = -1;


            int pos = obituaryText.LastIndexOf(" born ");
            if (pos >= 0)
            {
                int start = pos + " born ".Length;
                int length = Math.Min(60, obituaryText.Length - start);
                string snippet = obituaryText.Substring(start, length);
                ConsoleFormatter.WriteDebug($"Start date data:  {snippet}...");




            }
            else
            {
                ConsoleFormatter.WriteDebug("The word ' born ' was not found in the obituary text.");
            }

            return (yearOfBirth, yearOfDeath);
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
