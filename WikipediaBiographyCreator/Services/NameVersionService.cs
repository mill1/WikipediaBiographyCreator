using WikipediaBiographyCreator.Interfaces;

namespace WikipediaBiographyCreator.Services
{
    public class NameVersionService : INameVersionService
    {
        /// <summary>
        /// Returns different variations of a name based on firstname(s), lastname(s) and optional suffix
        /// </summary>
        /// <param name="firstnames"> First names, e.g. "John", "John Jack", "John J.", "J. J.", "John J. K.", "J. J. K."  ></param>
        /// <param name="surnames"> Surnames, e.g. "Rambo", "Rambo Matrix", "Rambo-Matrix"  ></param>
        /// <param name="suffix"> Suffix, e.g. "Jr.", "Sr.", "II", "4TH"  ></param>
        public List<string> GetNameVersions(string firstnames, string surnames, string suffix)
        {
            var results = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string BuildName(string f, string s, string suf) =>
                string.Join(" ", new[] { f, s, suf }.Where(x => !string.IsNullOrWhiteSpace(x)));

            // Split tokens
            var firstnameParts = firstnames.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var surnameParts = surnames.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Firstname variants: all parts, and only first part
            var allFirstnames = string.Join(" ", firstnameParts);
            var onlyFirst = firstnameParts.Length > 0 ? firstnameParts[0] : string.Empty;

            // Surname variants: all parts, and only first surname (strip after hyphen if present)
            var allSurnames = string.Join(" ", surnameParts);
            string onlyFirstSurname = string.Empty;
            if (surnameParts.Length > 0)
            {
                var firstSurnameToken = surnameParts[0];
                var hyphenIndex = firstSurnameToken.IndexOf('-');
                onlyFirstSurname = hyphenIndex > 0 ? firstSurnameToken.Substring(0, hyphenIndex) : firstSurnameToken;
            }

            // Build ordered variant lists (avoid adding identical variants twice)
            var surnameVariants = new List<string>();
            if (!string.IsNullOrWhiteSpace(allSurnames))
                surnameVariants.Add(allSurnames);

            if (!string.IsNullOrWhiteSpace(onlyFirstSurname)
                && !string.Equals(onlyFirstSurname, allSurnames, StringComparison.OrdinalIgnoreCase))
            {
                surnameVariants.Add(onlyFirstSurname);
            }

            var firstnameVariants = new List<string>();
            if (!string.IsNullOrWhiteSpace(allFirstnames))
                firstnameVariants.Add(allFirstnames);

            if (!string.IsNullOrWhiteSpace(onlyFirst)
                && !string.Equals(onlyFirst, allFirstnames, StringComparison.OrdinalIgnoreCase))
            {
                firstnameVariants.Add(onlyFirst);
            }

            // Iterate surname variants outer, firstname inner, suffix options inner-most
            foreach (var sVar in surnameVariants)
            {
                foreach (var fVar in firstnameVariants)
                {
                    if (string.IsNullOrWhiteSpace(fVar) && string.IsNullOrWhiteSpace(sVar))
                        continue;

                    if (!string.IsNullOrEmpty(suffix))
                    {
                        var withSuffix = BuildName(fVar, sVar, suffix);
                        if (seen.Add(withSuffix)) results.Add(withSuffix);

                        var withoutSuffix = BuildName(fVar, sVar, "");
                        if (seen.Add(withoutSuffix)) results.Add(withoutSuffix);
                    }
                    else
                    {
                        var name = BuildName(fVar, sVar, "");
                        if (seen.Add(name)) results.Add(name);
                    }
                }
            }

            return results;
        }
    }
}
