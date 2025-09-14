using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Exceptions;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;
using WikipediaBiographyCreator.Models.Guardian;

namespace WikipediaBiographyCreator.Services
{
    public class GuardianApiService : IGuardianApiService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IGuardianObituarySubjectService _obituarySubjectService;

        public GuardianApiService(
            IConfiguration configuration,
            HttpClient httpClient,
            IGuardianObituarySubjectService obituarySubjectService,
            IAssemblyService assemblyService)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _obituarySubjectService = obituarySubjectService;
            _httpClient.BaseAddress = new Uri("https://content.guardianapis.com");

            var assemblyName = assemblyService.GetAssemblyName();
            string agent = $"{assemblyName.Name} v.{assemblyName.Version}";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", agent);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public List<Obituary> ResolveObituariesOfMonth(int year, int monthId)
        {
            /* TODO
                "total": 74,
                "startIndex": 11,
                "pageSize": 10,  ALWAYS
                "currentPage": 2,
                "pages": 8,

                "total": 8,
                "startIndex": 1,
                "pageSize": 10,
                "currentPage": 1,
                "pages": 1,

             */

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
                        Title = result.webTitle,
                        Subject = _obituarySubjectService.Resolve(result)
                    }));


                if (page == pages)
                {
                    break;
                }
            }
            ;


            //for (int page = 1; page <= 8; page++)
            //{
            //    string json = GetResponse(year, monthId, page, apiKey);
            //    var obituaryResults = GetObituaryResults(json);

            //    obituaries.AddRange(
            //        obituaryResults.Select(result => new Obituary
            //        {
            //            Title = result.webTitle,
            //            Subject = _obituarySubjectService.Resolve(result)
            //        }));
            //}

            return obituaries;
        }

        private IEnumerable<Result> GetObituaryResults(string json, out int pages)
        {
            Rootobject archive = JsonConvert.DeserializeObject<Rootobject>(json);

            if (archive == null)
            {
                throw new AppException("Error deserializing Guardian archive JSON response.");
            }

            pages = archive.response.pages;

            return archive.response.results.ToList();
        }

        private string GetResponse(int year, int monthId, int page, string apiKey)
        {
            /*
                - Up to 1 call per second
                - Up to 500 calls per day
                - Access to article text
             */

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
            string apiKey = _configuration["Guardian:ApiKey"];

            if (apiKey == null || apiKey == "TOSET")
            {
                throw new AppException("Guardian API key is not set in appsettings.json. Consult the README.");
            }

            return apiKey;
        }
    }
}