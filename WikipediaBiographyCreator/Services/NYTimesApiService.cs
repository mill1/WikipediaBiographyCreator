using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Exceptions;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;
using WikipediaBiographyCreator.Models.NYTimes;

namespace WikipediaBiographyCreator.Services
{
    public class NYTimesApiService : INYTimesApiService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly INYTimesObituarySubjectService _obituarySubjectService;

        public NYTimesApiService(
            IConfiguration configuration,
            HttpClient httpClient,
            INYTimesObituarySubjectService obituarySubjectService,
            IAssemblyService assemblyService)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _obituarySubjectService = obituarySubjectService;
            _httpClient.BaseAddress = new Uri("https://api.nytimes.com");

            var assemblyName = assemblyService.GetAssemblyName();
            string agent = $"{assemblyName.Name} v.{assemblyName.Version}";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", agent);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public List<Obituary> ResolveObituariesOfMonth(int year, int monthId)
        {
            string json = GetResponse(year, monthId, GetApiKey());
            IEnumerable<Doc> articles = GetArticles(json);
            IEnumerable<Doc> obituaryDocs = GetObituaries(year, monthId, articles);

            var obituaries = obituaryDocs
                .Select(doc => new Obituary
                {
                    Id = doc._id,
                    Title = doc.headline.main,
                    WebUrl = doc.web_url,
                    Subject = _obituarySubjectService.Resolve(doc)
                })
                .GroupBy(o => o.Subject.NormalizedName).Select(grp => grp.First()) // Remove duplicates 
                .OrderBy(o => o.Subject.NormalizedName)
                .ToList();

            return obituaries;
        }

        private IEnumerable<Doc> GetArticles(string json)
        {
            Archive archive = JsonConvert.DeserializeObject<Archive>(json);

            if (archive == null)
            {
                throw new AppException("Error deserializing NYTimes archive JSON response.");
            }

            // Remove duplicates based on _id property
            var articles = archive.response.docs.GroupBy(d => d._id).Select(grp => grp.First());

            return articles;
        }

        private IEnumerable<Doc> GetObituaries(int year, int monthId, IEnumerable<Doc> articles)
        {
            try
            {
                return articles.Where(d => d.type_of_material.Contains("Obituary")).AsEnumerable().OrderBy(d => d.pub_date);
            }
            catch (Exception) // Not every articleDoc has a property type_of_material
            {
                return GetObituariesNoLinq(year, monthId, articles);
            }
        }

        private List<Doc> GetObituariesNoLinq(int year, int monthId, IEnumerable<Doc> articles)
        {
            var obituaries = new List<Doc>();

            foreach (var doc in articles)
            {
                try
                {
                    if (doc.type_of_material.Contains("Obituary"))
                    {
                        obituaries.Add(doc);
                    }
                }
                catch (Exception)
                {
                    ConsoleFormatter.WriteWarning($"Doc object has no property type_of_material. Year: {year} Month: {monthId} doc Id: {doc._id}");
                }
            }

            return obituaries;
        }

        private string GetResponse(int year, int monthId, string apiKey)
        {
            string uri = @"svc/archive/v1/" + @$"{year}/{monthId}.json?api-key={apiKey}";

            ConsoleFormatter.WriteInfo($"Retrieving NYTimes archive for {year}/{monthId}...");

            // by calling .Result you are synchronously reading the result
            var response = _httpClient.GetAsync(uri).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new AppException($"Error retrieving NYTimes archive: {response.ReasonPhrase}");

        }

        private string GetApiKey()
        {
            string apiKey = _configuration["NYTimes:ApiKey"];

            if (apiKey == null || apiKey == "TOSET")
            {
                throw new AppException("NYTimes API key is not set in appsettings.json. Consult the README.");
            }

            return apiKey;
        }
    }
}
