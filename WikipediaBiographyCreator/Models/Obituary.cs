namespace WikipediaBiographyCreator.Models
{
    public class Obituary
    {
        public required string Title { get; set; }

        public int Page { get; set; } // TMP

        public required Subject Subject { get; set; }
    }

    public class Subject
    {
        public required string Name { get; set; }
        public required List<string> NameVersions { get; set; }
        public int YearOfBirth { get; set; }
        public int YearOfDeath { get; set; }
    }
}
