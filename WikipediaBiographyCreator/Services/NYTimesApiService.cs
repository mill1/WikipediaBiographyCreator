using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Exceptions;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Services
{
    public class NYTimesApiService : INYTimesApiService
    {
        private readonly HttpClient _httpClient;
        private readonly INYTimesObituarySubjectService _obituarySubjectService;

        public NYTimesApiService(
            HttpClient httpClient,
            INYTimesObituarySubjectService obituarySubjectService,
            IAssemblyService assemblyService)
        {
            _httpClient = httpClient;
            _obituarySubjectService = obituarySubjectService;
            _httpClient.BaseAddress = new Uri("https://api.nytimes.com");

            var assemblyName = assemblyService.GetAssemblyName();
            string agent = $"{assemblyName.Name} v.{assemblyName.Version}";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", agent);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public List<Obituary> ResolveObituariesOfMonth(int year, int monthId, string apiKey)
        {
            string json = GetResponse(year, monthId, apiKey);
            IEnumerable<Doc> articleDocs = GetArticleDocs(json);
            IEnumerable<Doc> obituaryDocs = GetObituaryDocs(year, monthId, articleDocs);

            var obituaries = obituaryDocs.Select(doc => new Obituary
            {
                Title = doc.headline.main, // TODO checken wat is dit btw?: doc.headline.print_headline ?? doc.headline.main,
                Subject = _obituarySubjectService.Resolve(doc)
            }).OrderBy(o => o.Subject.Name).ToList();

            return obituaries;
        }

        private IEnumerable<Doc> GetArticleDocs(string json)
        {
            NYTimesArchive archive = JsonConvert.DeserializeObject<NYTimesArchive>(json);

            if (archive == null)
            {
                throw new AppException("Error deserializing NYTimes archive JSON response.");
            }

            // Remove duplicates based on _id property
            var articleDocs = archive.response.docs.GroupBy(d => d._id).Select(grp => grp.First());

            return articleDocs;
        }

        private IEnumerable<Doc> GetObituaryDocs(int year, int monthId, IEnumerable<Doc> articleDocs)
        {
            try
            {
                return articleDocs.Where(d => d.type_of_material.Contains("Obituary")).AsEnumerable().OrderBy(d => d.pub_date);
            }
            catch (Exception) // Not every articleDoc has a property type_of_material
            {
                return GetObituaryDocsNoLinq(year, monthId, articleDocs);
            }
        }

        private List<Doc> GetObituaryDocsNoLinq(int year, int monthId, IEnumerable<Doc> articleDocs)
        {
            var obituaryDocs = new List<Doc>();

            foreach (var doc in articleDocs)
            {
                try
                {
                    if (doc.type_of_material.Contains("Obituary"))
                    {
                        obituaryDocs.Add(doc);
                    }
                }
                catch (Exception)
                {
                    ConsoleFormatter.WriteWarning($"Doc object has no property type_of_material. Year: {year} Month: {monthId} doc Id: {doc._id}");
                }
            }

            return obituaryDocs;
        }

        private string GetResponse(int year, int monthId, string apiKey)
        {
            // string uri = @"https://api.nytimes.com/svc/archive/v1/" + @$"{year}/{monthId}.json?api-key={apiKey}";
            string uri = @"svc/archive/v1/" + @$"{year}/{monthId}.json?api-key={apiKey}";

            ConsoleFormatter.WriteInfo($"Retrieving NYTimes archive for {year}/{monthId}...");

            // by calling .Result you are synchronously reading the result
            var response = _httpClient.GetAsync(uri).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new AppException($"Error retrieving NYTimes archive: {response.ReasonPhrase}");

        }
    }
}
