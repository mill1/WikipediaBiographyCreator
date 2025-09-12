namespace WikipediaBiographyCreator.Exceptions
{
    public class SignupException : Exception
    {
        // Constructor that accepts a message
        public SignupException(string message) : base(message)
        {
        }
    }
}