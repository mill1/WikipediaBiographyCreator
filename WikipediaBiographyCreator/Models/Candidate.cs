namespace WikipediaBiographyCreator.Models
{
    public class Candidate
    {
        public required string Name { get; set; }
        public string? WebUrlGuardian { get; set; }
        public string? WebUrlNYTimes { get; set; }

        public override string ToString()
        {
            return $"{Name}\r\n{WebUrlGuardian}\r\n{WebUrlNYTimes}";
        }
    }
}
