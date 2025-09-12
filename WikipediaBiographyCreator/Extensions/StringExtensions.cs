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
    }
}