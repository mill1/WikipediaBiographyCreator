using System.Text.RegularExpressions;

namespace WikipediaBiographyCreator.Extensions
{
    public static class StringExtensions
    {
        public static string PadMiddle(this string str, int TotalWidth, char paddingChar)
        {
            int numberOfPrecedingPaddingChars = (TotalWidth - str.Length) / 2;
            int numberOfTrailingPaddingChars = TotalWidth - numberOfPrecedingPaddingChars - str.Length;

            return new String(paddingChar, numberOfPrecedingPaddingChars) + str + new String(paddingChar, numberOfTrailingPaddingChars);
        }

        public static string CapitalizeName(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;

            str = str.Trim();
            var words = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string suffix = string.Empty;

            // Roman numeral check (kept from earlier)
            var romanRegex = new Regex("^(M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3}))$",
                                       RegexOptions.IgnoreCase);

            if (romanRegex.IsMatch(words.Last()))
            {
                suffix = " " + words.Last(); // preserve as-is
                words = words.Take(words.Length - 1).ToArray();
            }

            var capitalizedWords = words.Select(word =>
            {
                var subWords = word.Split('-', StringSplitOptions.RemoveEmptyEntries);

                var capitalizedSubWords = subWords.Select(CapitalizeWordWithMcMac);

                return string.Join("-", capitalizedSubWords);
            });

            string name = string.Join(' ', capitalizedWords) + suffix;

            // TODO see x
            // Final tweaks
            name = name.Replace(" De ", " de "); // Maurice Couve de Murville
            name = name.Replace(" Da ", " da "); // Neuma Goncalves da Silva, Francisco Da Costa Gomes
            name = name.Replace(" Von ", " von "); // Freya von Moltke
            name = name.Replace(" Van ", " van ");
            name = name.Replace(" Der ", " der ");
            name = name.Replace(" La ", " la "); //Miguel de la Madrid

            return name;
        }

        private static string CapitalizeWordWithMcMac(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            // Basic capitalization
            string result = char.ToUpper(word[0]) + word.Substring(1).ToLower();

            // Special handling for McX...
            if (result.StartsWith("Mc") && result.Length > 2)
            {
                result = "Mc" + char.ToUpper(result[2]) + result.Substring(3);
            }
            // Special handling for MacX... (Teo Macero :( )
            else if (result.StartsWith("Mac") && result.Length > 3)
            {
                result = "Mac" + char.ToUpper(result[3]) + result.Substring(4);
            }
            // Special handling for O'X...
            else if (result.StartsWith("O'") && result.Length > 2)
            {
                result = "O'" + char.ToUpper(result[2]) + result.Substring(3);
            }

            return result;
        }
    }
}
