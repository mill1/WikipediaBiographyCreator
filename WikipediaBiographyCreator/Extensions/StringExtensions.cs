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

        public static string Capitalize(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;

            str = str.Trim();
            var words = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string suffix = string.Empty;

            // Regex for Roman numerals up to 3999 (you can simplify if needed)
            var romanRegex = new Regex("^(M{0,4}(CM|CD|D?C{0,3})" +
                                       "(XC|XL|L?X{0,3})(IX|IV|V?I{0,3}))$",
                                       RegexOptions.IgnoreCase);

            if (romanRegex.IsMatch(words.Last()))
            {
                suffix = " " + words.Last(); // preserve as-is
                words = words.Take(words.Length - 1).ToArray();
            }

            var capitalizedWords = words.Select(word =>
            {
                var subWords = word.Split('-', StringSplitOptions.RemoveEmptyEntries);

                var capitalizedSubWords = subWords.Select(sw =>
                    char.ToUpper(sw[0]) + sw.Substring(1).ToLower()
                );

                return string.Join("-", capitalizedSubWords);
            });

            return string.Join(' ', capitalizedWords) + suffix;
        }
    }
}
