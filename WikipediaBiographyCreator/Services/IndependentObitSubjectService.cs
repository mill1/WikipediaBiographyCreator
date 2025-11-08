using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;

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
            //var bornPattern = new Regex(@"\bborn\b.*?(?:(\d{1,2})\s+)?([A-Za-z]+)\s+(\d{4})", RegexOptions.IgnoreCase);
            //var diedPattern = new Regex(@"\bdied\b.*?(?:(\d{1,2})\s+)?([A-Za-z]+)\s+(\d{4})", RegexOptions.IgnoreCase);
            var bornPattern = new Regex(@"\bborn\b[^;:.]*?(?:(\d{1,2})\s+([A-Za-z]+)\s+(\d{4}))", RegexOptions.IgnoreCase);
            var diedPattern = new Regex(@"\bdied\b[^;:.]*?(?:(\d{1,2})\s+([A-Za-z]+)\s+(\d{4}))", RegexOptions.IgnoreCase);


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

        public (DateOnly PublicationDate, string Title, string Description) ExtractMetadata(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Helper to get meta content by either "property" or "name"
            string? GetMetaContent(string attrName, string attrValue)
            {
                return doc.DocumentNode
                    .SelectSingleNode($"//meta[@{attrName}='{attrValue}']")
                    ?.GetAttributeValue("content", null);
            }

            // Try all possible date sources
            string? dateStr =
                GetMetaContent("property", "article:published_time") ??
                GetMetaContent("property", "og:updated_time") ??
                GetMetaContent("property", "date");

            // Title (almost always under og:title)
            string? title =
                GetMetaContent("property", "og:title") ??
                doc.DocumentNode.SelectSingleNode("//title")?.InnerText;

            // Description
            string? description =
                GetMetaContent("name", "description") ??
                GetMetaContent("property", "og:description");

            // Parse ISO date safely to DateOnly
            DateOnly publicationDate = DateOnly.FromDateTime(
                DateTime.Parse(dateStr!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
            );

            return (publicationDate, title ?? string.Empty, description ?? string.Empty);
        }

        public Subject Resolve(Obituary obituary)
        {
            throw new NotImplementedException();
        }
    }
}
