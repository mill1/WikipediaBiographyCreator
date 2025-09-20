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

            // Split words on spaces
            var words = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var capitalizedWords = words.Select(word =>
            {
                // Split subwords on hyphens
                var subWords = word.Split('-', StringSplitOptions.RemoveEmptyEntries);

                // Capitalize each subword
                var capitalizedSubWords = subWords.Select(sw =>
                    char.ToUpper(sw[0]) + sw.Substring(1).ToLower()
                );

                // Join subwords back with hyphen
                return string.Join("-", capitalizedSubWords);
            });

            // Join words back with space
            return string.Join(' ', capitalizedWords);
        }
    }
}
