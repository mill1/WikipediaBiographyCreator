using System.Globalization;
using System.Text.RegularExpressions;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;
using WikipediaBiographyCreator.Models.Guardian;

namespace WikipediaBiographyCreator.Services
{
    public class IndependentObitSubjectService : IIndependentObitSubjectService
    {
        public (DateOnly DateOfBirth, DateOnly DateOfDeath) ResolveDoBAndDoD(string obituaryText)
        {
            if (string.IsNullOrWhiteSpace(obituaryText))
                return (default, default);

            // Normalize spaces
            string text = obituaryText.Replace("\n", " ").Replace("\r", " ");

            // Remove parenthetical text — prevents false "died" matches
            text = Regex.Replace(text, @"\([^)]*\)", string.Empty);

            // --- Regex patterns ---
            var bornPattern = new Regex(@"\bborn\b[^.,;:]*?\b(?:(\d{1,2})\s+)?([A-Za-z]+)\s+(\d{4})", RegexOptions.IgnoreCase);
            var diedPattern = new Regex(@"\bdied\b[^.,;:]*?\b(?:(\d{1,2})\s+)?([A-Za-z]+)\s+(\d{4})", RegexOptions.IgnoreCase);

            // --- Extract birth ---
            DateOnly dob = default;
            var bornMatch = bornPattern.Match(text);
            if (bornMatch.Success)
                dob = ParseDate(bornMatch);

            // --- Extract death ---
            DateOnly dod = default;
            var diedMatch = diedPattern.Match(text);
            if (diedMatch.Success)
                dod = ParseDate(diedMatch);

            return (dob, dod);
        }

        private static DateOnly ParseDate(Match m)
        {
            var dayPart = m.Groups[1].Success ? m.Groups[1].Value : "1"; // default to day=1 if missing
            var monthPart = m.Groups[2].Value;
            var yearPart = m.Groups[3].Value;

            var dateString = $"{dayPart} {monthPart} {yearPart}";

            if (DateTime.TryParseExact(
                    dateString,
                    new[] { "d MMMM yyyy", "d MMM yyyy" },
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var dt))
            {
                return DateOnly.FromDateTime(dt);
            }

            return default;
        }

        public Subject Resolve(Result obituary)
        {
            throw new NotImplementedException();
        }
    }
}
