namespace WikipediaBiographyCreator.Exceptions
{
    public class AppException : Exception
    {
        // Constructor that accepts a message
        public AppException(string message) : base(message)
        {
        }
    }
}