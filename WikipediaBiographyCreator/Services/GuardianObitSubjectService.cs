using System.Globalization;
using System.Text.RegularExpressions;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;
using WikipediaBiographyCreator.Models.Guardian;

namespace WikipediaBiographyCreator.Services
{
    public class GuardianObitSubjectService : IGuardianObitSubjectService
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

        public (DateOnly DateOfBirth, DateOnly DateOfDeath) ResolveDoBAndDoD(string obituaryText)
        {
            DateOnly dateOfBirth = DateOnly.MinValue;
            DateOnly dateOfDeath = DateOnly.MinValue;

            int pos = obituaryText.LastIndexOf(" born ");
            if (pos >= 0)
            {
                // Get the 60 characters after the last occurence of word ' born ' in the obituary text.
                int start = pos + " born ".Length;
                int length = Math.Min(60, obituaryText.Length - start);
                string snippet = obituaryText.Substring(start, length);

                if (snippet.Contains(" died,"))
                    ConsoleFormatter.WriteDebug($"Snippet not well formed (' died,'): {snippet}");

                /*
                    Many Guardian obituary texts end stating the DoB and DoD in these exact format:
                    [Name], [profession], born [DoB]; died [DoD]  e.g.
                    Giorgio Armani, fashion designer, born 11 July 1934; died 4 September 2025

                    Examples of snippets found:
                    Start date data:  September 13, 1922; died January 21, 1999
                    Start date data:  on an Oklahoma farm. His mother was a Cree, his father a Che...
                    Start date data:  January 19 1919; died November 18 1999
                    Start date data:  October 1, 1917; died January 12, 1999 Godfrey Hodgson...
                */

                if (!snippet.Contains(" died "))
                    return (dateOfBirth, dateOfDeath);

                var regex = new Regex(
                    @"(?<dob>(?:\d{1,2}\s+[A-Za-z]+|\b[A-Za-z]+\s+\d{1,2}),?\s+\d{4})\s*;\s*died\s+(?<dod>(?:\d{1,2}\s+[A-Za-z]+|\b[A-Za-z]+\s+\d{1,2}),?\s+\d{4})",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                var match = regex.Match(snippet);
                if (match.Success)
                {
                    if (DateTime.TryParse(match.Groups["dob"].Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob))
                        dateOfBirth = DateOnly.FromDateTime(dob);

                    if (DateTime.TryParse(match.Groups["dod"].Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dod))
                        dateOfDeath = DateOnly.FromDateTime(dod);
                }
            }

            return (dateOfBirth, dateOfDeath);
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

            // Loretta Young - actress
            pos = subjectName.IndexOf(" - ");
            if (pos > 0)
                subjectName = subjectName.Substring(0, pos).Trim();

            // Lynden Pindling, Bahamian politician
            pos = subjectName.IndexOf(", ");
            if (pos > 0)
                subjectName = subjectName.Substring(0, pos).Trim();

            // One-offs
            subjectName = subjectName.Replace("Obituaries; ", string.Empty);
            subjectName = subjectName.Replace(" (letter); ", string.Empty);

            return SanitizeSubjectName(subjectName);
        }

        private static string GetNormalizedName(string subjectName)
        {
            // Add a point behind any single uppercase letters (A-Z) except when it is situated at the end.
            var normalizedName = Regex.Replace(
                subjectName,
                @"\b([A-Z])\b(?!$)",
                "$1."
            );

            return normalizedName;
        }

        private static string SanitizeSubjectName(string subjectName)
        {
            // 1999 - 200?: webTitle = "Wally Cole obituary" -> "Wally Cole"
            var name = subjectName.Replace(" obituary", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

            // More sanitization; remove precedings titles like 'Sir', 'Dame', et cetera. Add a space after each title to prevent removing parts of names.
            var titles = new List<string>
            {
                "Sir ",
                //"Dame ",
                //"Lord ",
                //"Lady ",
                //"Count",
                //"Countess ",
                //"Baron ",
                //"Baroness ",
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
