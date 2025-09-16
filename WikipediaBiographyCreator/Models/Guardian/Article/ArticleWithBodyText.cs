namespace WikipediaBiographyCreator.Models.Guardian.Article
{
    public class ArticleWithBodyText
    {
        public Response response { get; set; }
    }

    public class Response
    {
        public string status { get; set; }
        public string userTier { get; set; }
        public int total { get; set; }
        public Content content { get; set; }
    }

    public class Content
    {
        public string id { get; set; }
        public string type { get; set; }
        public string sectionId { get; set; }
        public string sectionName { get; set; }
        public DateTime webPublicationDate { get; set; }
        public string webTitle { get; set; }
        public string webUrl { get; set; }
        public string apiUrl { get; set; }
        public Fields fields { get; set; }
        public bool isHosted { get; set; }
        public string pillarId { get; set; }
        public string pillarName { get; set; }
    }

    public class Fields
    {
        public string bodyText { get; set; }
    }

}
