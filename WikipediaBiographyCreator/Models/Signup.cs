namespace WikipediaBiographyCreator.Models
{
    public class Signup
    {
        public string Id { get; set; }

        public string? Name { get; set; }

        public string? PhoneNumber { get; set; }

        public int PartySize { get; set; }

        public override string? ToString()
        {
            return $"Id {Id}: {Name} {PhoneNumber} ({PartySize})";
        }
    }
}
