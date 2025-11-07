namespace WikipediaBiographyCreator.Models
{
    public class Candidate
    {
        public required string Name { get; set; }
        public string? WebUrl { get; set; }
        public string? WebUrlNYTimes { get; set; }

        public override string ToString()
        {
            return $"{Name}\r\n{WebUrl}\r\n{WebUrlNYTimes}";
        }
    }
}
