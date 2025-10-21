using System.Xml.Linq;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models.Guardian;

namespace WikipediaBiographyCreator.Services
{
    public class NameVersionService : INameVersionService
    {
        /// <summary>
        /// Returns different variations of a name based on firstname(s), lastname(s) and optional suffix
        /// </summary>
        /// <param name="firstnames"> First names, e.g. "John", "John Jack", "John J.", "J. J.", "John J. K.", "J. J. K."  ></param>
        /// <param name="surname"> Surname (by marriage), e.g. "Rambo". Maiden name is the last value in firstnames ></param>
        /// <param name="suffix"> Suffix, e.g. "Jr.", "Sr.", "II"  ></param>
        public List<string> GetNameVersions(string firstnames, string surname, string suffix)
        {
            var results = new List<string>();

            string BuildName(string f, string s, string suf) =>
                string.Join(" ", new[] { f, s, suf }.Where(x => !string.IsNullOrWhiteSpace(x)));

            // Split firstname tokens
            var firstnameParts = firstnames.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Two firstname variants:
            var allFirstnames = string.Join(" ", firstnameParts); // all parts
            var onlyFirst = firstnameParts.Length > 0 ? firstnameParts[0] : string.Empty; // first only

            // Helper to add versions with and without suffix
            void AddWithSuffixOptions(string f, string s)
            {
                if (string.IsNullOrWhiteSpace(f) && string.IsNullOrWhiteSpace(s))
                    return;

                results.Add(BuildName(f, s, suffix));
                if (!string.IsNullOrEmpty(suffix))
                    results.Add(BuildName(f, s, ""));
            }

            // Add "all firstnames" version(s)
            if (!string.IsNullOrWhiteSpace(allFirstnames))
                AddWithSuffixOptions(allFirstnames, surname);

            // Add "only first firstname" version(s) if different
            if (!string.IsNullOrWhiteSpace(onlyFirst) && onlyFirst != allFirstnames)
                AddWithSuffixOptions(onlyFirst, surname);

            for (int i = 0; i < results.Count; i++)
            {
                // TODO see x
                // Final tweaks
                results[i] = results[i].Replace(" De ", " de "); // Maurice Couve de Murville
                results[i] = results[i].Replace(" Da ", " da "); // Neuma Goncalves da Silva, Francisco Da Costa Gomes
                results[i] = results[i].Replace(" Von ", " von "); // Freya von Moltke
                results[i] = results[i].Replace(" Van ", " van ");
                results[i] = results[i].Replace(" Der ", " der ");
                results[i] = results[i].Replace(" La ", " la "); //Miguel de la Madrid
            }

            return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
