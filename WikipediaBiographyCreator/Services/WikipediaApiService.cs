using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using WikipediaBiographyCreator.Exceptions;
using WikipediaBiographyCreator.Interfaces;

namespace WikipediaBiographyCreator.Services
{
    public class WikipediaApiService : IWikipediaApiService
    {
        private readonly HttpClient _httpClient;

        public WikipediaApiService(
            IConfiguration configuration,
            HttpClient httpClient,
            IAssemblyService assemblyService)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://en.wikipedia.org");

            var assemblyName = assemblyService.GetAssemblyName();
            string agent = $"{assemblyName.Name} v.{assemblyName.Version}";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", agent);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public string GetWikipediaPageTitle(string nameVersion)
        {
            // Call Wikipedia API to check if an article with a given name exists

            JObject obj = GetResponse(nameVersion);

            var pages = obj["query"]?["pages"];

            foreach (var page in pages)
            {
                var pageObj = (JProperty)page;
                var pageId = pageObj.Name;

                if (pageId == "-1")
                {
                    // No article for that name/title version
                    return string.Empty;
                }

                // In case of a proper id we have props
                var props = pageObj.Value["pageprops"];

                if (props != null && props["disambiguation"] != null)
                {
                    // TODO disamb
                    return nameVersion;
                }
                else
                {
                    // Redirected?
                    var redirects = obj["query"]?["redirects"];

                    if (redirects == null)
                    {
                        return nameVersion;
                    }
                    else
                    {
                        if (!redirects.HasValues)
                            throw new AppException($"{nameVersion}: redirect without values; investigate");
                        string toValue = redirects[0]?["to"]?.ToString();
                        return $"{toValue} [redirected]";
                    }
                }
            }


            return string.Empty;
        }

        private JObject GetResponse(string nameVersion)
        {
            string url = $"w/api.php?action=query&titles={Uri.EscapeDataString(nameVersion)}&prop=pageprops&redirects=1&format=json";

            string json = _httpClient.GetStringAsync(url).Result;
            return JObject.Parse(json);
        }
    }
}
