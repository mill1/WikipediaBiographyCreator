using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Extensions;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;
using WikipediaBiographyCreator.Models.NYTimes;

namespace WikipediaBiographyCreator.Services
{
    public class NYTimesObituarySubjectService : INYTimesObituarySubjectService
    {
        private readonly INameVersionService _nameVersionService;

        public NYTimesObituarySubjectService(INameVersionService nameVersionService)
        {
            _nameVersionService = nameVersionService;
        }

        public Subject Resolve(Doc doc)
        { 
            var subjectName = ResolveSubjectName(doc);

            return new Subject
            {
                Name = subjectName,
                NormalizedName = GetNameVersions(subjectName).OrderBy(nv => nv.Length).Last(),
            };
        }

        public List<string> GetNameVersions(string subjectName)
        {
            int i = subjectName.IndexOf(",");

            if (i == -1) // Just one name or new format : 'John J. Rambo' instead of 'Rambo, John J.'
                return new List<string> { subjectName.CapitalizeName() };

            // "BAUMFELD," in request March 1988
            if (!subjectName.Contains(' '))
                return new List<string> { subjectName.Replace(",", "").CapitalizeName() };

            string surname = subjectName.Substring(0, i).CapitalizeName();

            string firstnames = subjectName.Substring(i + 1).Trim();
            firstnames = AdjustFirstNames(firstnames, out string suffix);

            return _nameVersionService.GetNameVersions(firstnames, surname, suffix);
        }

        private string ResolveSubjectName(Doc doc)
        {
            var persons = doc.keywords.Where(k => k.name == "persons").Select(p => p.value).ToList();

            if (persons.Any())
            {
                persons.ForEach(p =>
                {
                    // Loose paranthesis stuff;  Strand, Mark (1934-2014) should become Strand, Mark
                    int pos = p.IndexOf('(');
                    if (pos > 0)
                        p = p.Substring(0, pos).Trim();
                });

                // If multiple persons are listed, we pick the one best matching the headline
                var bestMatch = FuzzySharp.Process.ExtractOne(doc.headline.main, persons);
                string subjectName = bestMatch.Value;

                if (subjectName != persons[0])
                {
                    int maxLength = Math.Min(40, doc.headline.main.Length);
                    ConsoleFormatter.WriteError($"\"{subjectName}\", not \"{persons[0]}\"! Main: {doc.headline.main.Substring(0, maxLength)}");
                }

                return subjectName;
            }
            else
            {
                // ConsoleFormatter.WriteError($"Id: {doc._id} Error: keywords; \"name\": \"persons\" is missing");

                int pos = doc.headline.main.IndexOf(',');

                if (pos < 0)
                {
                    int maxLength = Math.Min(40, doc.headline.main.Length);

                    return $"Name cannot be resolved. Main: {doc.headline.main.Substring(0, maxLength)}";
                }

                return doc.headline.main.Substring(0, pos);
            }
        }

        private string AdjustFirstNames(string firstnames, out string suffix)
        {
            suffix = string.Empty;

            if (firstnames.Length < 3)
                return firstnames;

            firstnames = CapitalizeFirstNamesAndCutSuffix(firstnames, ref suffix);

            // Add dot after initials, if any
            var names = firstnames.Split(' ');

            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].Length == 1)
                    names[i] = names[i] + ".";
            }
            firstnames = string.Join(" ", names);

            return firstnames;
        }

        /// <summary>
        /// Cut suffixes which are not part of the first name(s) but should be put at the end of the name.
        /// </summary>
        /// <param name="firstnames">Examples firstnames: "RAMBO, JOHN", "ROCKEFELLER, JOHN 3D", "JOHN R. DOE"</param>
        /// <param name="suffix">Examples suffixes: JR, SR, II, III, .. X, 2D, 3D, 4TH .. 9TH</param>
        /// <returns></returns>
        private static string CapitalizeFirstNamesAndCutSuffix(string firstnames, ref string suffix)
        {
            if (string.IsNullOrWhiteSpace(firstnames))
                return string.Empty;

            // Normalize input
            firstnames = firstnames.Trim();

            // Split into parts
            var parts = firstnames.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var tail = parts.Last().ToUpperInvariant();

            // Direct mappings (simple suffix -> replacement)
            // 'I' is kept since it can also mean I. (e.g. SAULS, JOHN I -> John I. Sauls). Same with V and X (OGASAWARA, FRANK X)
            var directMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["JR"] = "Jr.",
                ["SR"] = "Sr.",
                ["II"] = "II",
                ["III"] = "III",
                ["IV"] = "IV",
                ["VI"] = "VI",
                ["VII"] = "VII",
                ["VIII"] = "VIII",
                ["IX"] = "IX"
            };

            // Ordinal/number-based mappings
            var ordinalMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["1T"] = "I",
                ["1ST"] = "I",
                ["2D"] = "II",
                ["2ND"] = "II",
                ["3D"] = "III",
                ["3RD"] = "III",
                ["4TH"] = "IV",
                ["5TH"] = "V",
                ["6TH"] = "VI",
                ["7TH"] = "VII",
                ["8TH"] = "VIII",
                ["9TH"] = "IX",
                ["10TH"] = "X"
            };

            if (directMappings.TryGetValue(tail, out var mappedSuffix) ||
                ordinalMappings.TryGetValue(tail, out mappedSuffix))
            {
                suffix = mappedSuffix;
                parts = parts.Take(parts.Length - 1).ToArray();
            }
            else
            {
                // No suffix: just normalize capitalization
                suffix = string.Empty;
            }

            var normalized = string.Join(" ", parts).CapitalizeName();
            return normalized;
        }
    }
}
