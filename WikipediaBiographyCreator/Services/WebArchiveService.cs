using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Exceptions;
using WikipediaBiographyCreator.Interfaces;

namespace WikipediaBiographyCreator.Services
{
    public class WebArchiveService : IWebArchiveService
    {
        private readonly HttpClient _httpClient;

        public WebArchiveService(HttpClient httpClient, IAssemblyService assemblyService)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://web.archive.org");

            var assemblyName = assemblyService.GetAssemblyName();
            string agent = $"{assemblyName.Name} v.{assemblyName.Version}";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", agent);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public IEnumerable<string> ResolveUrlsTheIndependent()
        {
            const int Url = 2;
            string json = GetResponse();

            var archive = JsonConvert.DeserializeObject<string[][]>(json);

            if (archive == null)
                return new List<string> {"Search yielded no results"};

            return archive.Select(x => x[Url]).Skip(1);
        }

        public string GetResponse()
        {
            // Get the scraped pages of url's starting with 'www.independent.co.uk/incoming/obituary-'
            // Additional parameters: existing pages only (200), scraped since July 7, 2024 and remove duplicate scraped results.
            // This is only a small part of the available independent obits since over time they resided in different directories in the domain:
            // - www.independent.co.uk/news/obituaries/
            // - www.independent.co.uk/news/people/obituary-
            // - www.independent.co.uk/arts-entertainment/obituary-
            // - www.independent.co.uk/incoming/obituary-
            string uri = $"cdx/search/cdx?url=www.independent.co.uk/incoming/obituary-*&output=json&filter=statuscode:200&from=20240707&collapse=urlkey";

            ConsoleFormatter.WriteInfo($"Retrieving data from wayback machine...");

            // by calling .Result you are synchronously reading the result
            var response = _httpClient.GetAsync(uri).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new AppException($"Error retrieving web archive: {response.ReasonPhrase}");
        }
    }
}
