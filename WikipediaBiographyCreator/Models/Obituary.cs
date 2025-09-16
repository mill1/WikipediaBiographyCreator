namespace WikipediaBiographyCreator.Models
{
    public class Obituary
    {
        public required string Title { get; set; }
        public required Subject Subject { get; set; }
        public string ApiUrl { get; set; }
    }

    public class Subject
    {
        public required string Name { get; set; }
        public required string NormalizedName { get; set; }
    }
}
