using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Exceptions;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;
using WikipediaBiographyCreator.Models.Guardian;
using WikipediaBiographyCreator.Models.Guardian.Article;

namespace WikipediaBiographyCreator.Services
{
    public class GuardianApiService : IGuardianApiService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IGuardianObitSubjectService _obituarySubjectService;

        private static readonly HashSet<string> ExcludedTitles =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Letter",
                "Letters",
                "Obituary: Letter",
                "Obituaries: Letter",
                "Obituary: Letters",
                "Obituaries: Letters"
            };

        public GuardianApiService(
            IConfiguration configuration,
            HttpClient httpClient,
            IGuardianObitSubjectService obituarySubjectService,
            IAssemblyService assemblyService)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _obituarySubjectService = obituarySubjectService;
            _httpClient.BaseAddress = new Uri(_configuration["GuardianApi:BaseUrl"]);

            var assemblyName = assemblyService.GetAssemblyName();
            string agent = $"{assemblyName.Name} v.{assemblyName.Version}";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", agent);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public List<Obituary> ResolveObituariesOfMonth(int year, int monthId)
        {
            var obituaries = new List<Obituary>();
            var apiKey = GetApiKey();
            int page = 0;

            while (true)
            {
                page++;
                string json = GetResponse(year, monthId, page, apiKey);
                var obituaryResults = GetObituaryResults(json, out int pages);

                obituaries.AddRange(
                    obituaryResults.Select(result => new Obituary
                    {
                        Id = result.id,
                        Source = "Guardian",
                        PublicationDate = DateOnly.FromDateTime(result.webPublicationDate),
                        Title = result.webTitle,
                        WebUrl = result.webUrl,
                        FullTextUrl = result.apiUrl,
                        Subject = _obituarySubjectService.Resolve(result)
                    }));

                if (page == pages)
                    break;
            }

            obituaries = obituaries
                .GroupBy(o => o.Subject.NormalizedName).Select(grp => grp.First()) // Remove duplicates 
                .OrderBy(o => o.Subject.NormalizedName)
                .ToList();

            return obituaries;
        }

        public string GetObituaryText(string apiUrl, string subjectName)
        {
            var apiKey = GetApiKey();

            string json = GetResponse(apiUrl, apiKey, subjectName);
            var bodyText = GetObituaryBodyText(json);

            return bodyText;
        }

        private static string GetObituaryBodyText(string json)
        {
            ArticleWithBodyText article = JsonConvert.DeserializeObject<ArticleWithBodyText>(json);

            if (article == null)
            {
                throw new AppException("Error deserializing Guardian article JSON response.");
            }

            return article.response.content.fields.bodyText;
        }

        private IEnumerable<Result> GetObituaryResults(string json, out int pages)
        {
            Archive archive = JsonConvert.DeserializeObject<Archive>(json);

            if (archive == null)
            {
                throw new AppException("Error deserializing Guardian archive JSON response.");
            }

            pages = archive.response.pages;

            /*
             https://www.theguardian.com/news/2000/jan/24/guardianobituaries1
             https://www.theguardian.com/news/2001/mar/10/guardianobituaries
             */
            return archive.response.results
                .Where(r => r.type == "article" && !ExcludedTitles.Contains(r.webTitle))
                .ToList();
        }

        private string GetResponse(string apiUrl, string apiKey, string subjectName)
        {
            string uri = $"{apiUrl}?show-fields=bodyText&api-key={apiKey}";

            var response = _httpClient.GetAsync(uri).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new AppException($"Error retrieving Guardian obituary: {response.ReasonPhrase}");
        }

        private string GetResponse(int year, int monthId, int page, string apiKey)
        {
            int daysInMonth = DateTime.DaysInMonth(year, monthId);
            string uri = $"tone/obituaries?type=article&from-date={year}-{monthId}-1&to-date={year}-{monthId}-{daysInMonth}&page={page}&api-key={apiKey}";

            ConsoleFormatter.WriteInfo($"Retrieving page {page} of Guardian obituaries for {year}/{monthId}...");

            // by calling .Result you are synchronously reading the result
            var response = _httpClient.GetAsync(uri).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new AppException($"Error retrieving Guardian archive: {response.ReasonPhrase}");
        }

        private string GetApiKey()
        {
            string apiKey = _configuration["GuardianApi:ApiKey"];

            if (apiKey == null || apiKey == "YOUR_GUARDIAN_API_KEY")
            {
                throw new AppException("Guardian API key is not set in appsettings.json. Consult the README.");
            }

            return apiKey;
        }
    }
}