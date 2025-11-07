namespace WikipediaBiographyCreator.Models
{
    public class Obituary
    {
        public required string Id { get; set; }
        public required string Source { get; set; }
        public required string Title { get; set; }
        public required Subject Subject { get; set; }
        public required string WebUrl { get; set; }
        public required string FullTextUrl { get; set; }
        public required DateOnly PublicationDate { get; set; }
    }

    public class Subject
    {
        public required string Name { get; set; }
        public required string NormalizedName { get; set; }
    }
}
