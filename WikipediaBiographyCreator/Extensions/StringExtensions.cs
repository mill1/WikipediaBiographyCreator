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

            // Split words, ignore multiple spaces
            var words = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Capitalize each word
            var capitalizedWords = words.Select(w =>
                char.ToUpper(w[0]) + w.Substring(1).ToLower()
            );

            return string.Join(' ', capitalizedWords);
        }
    }
}