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
            _httpClient.BaseAddress = new Uri(configuration["WikipediaApi:BaseUrl"]);

            var assemblyName = assemblyService.GetAssemblyName();
            string agent = $"{assemblyName.Name} v.{assemblyName.Version}";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", agent);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public string GetPageTitle(string nameVersion, out bool disambiguation)
        {
            // Call Wikipedia API to check if an article with a given name exists
            disambiguation = false;

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

                // In case of a proper id we always have props
                var props = pageObj.Value["pageprops"];

                if (props != null && props["disambiguation"] != null)
                {
                    // Names versions are checked from most specific to more generic.
                    // If we end up here we can be sure that no 'normal' page will be found in subsequent versions,
                    // also because of the redirects involved,
                    disambiguation = true;
                    return nameVersion;
                }
                else
                {
                    // Normal article found. Was it redirected?
                    var redirects = obj["query"]?["redirects"];

                    if (redirects == null)
                    {
                        return nameVersion;
                    }
                    else
                    {
                        return ResolveRedirectedPageTitle(nameVersion, redirects);
                    }
                }
            }

            return string.Empty;
        }

        public string GetPageContent(string pageName)
        {
            string url = $"?action=query&titles={Uri.EscapeDataString(pageName)}&prop=pageprops&redirects=1&format=json";

            url = $"?action=query&prop=revisions&rvprop=content&rvslots=*&titles={Uri.EscapeDataString(pageName)}&format=json";

            string json = _httpClient.GetStringAsync(url).Result;
            var obj = JObject.Parse(json);

            var pages = obj["query"]?["pages"];

            foreach (var page in pages.Children<JProperty>())
            {
                var content = page.Value["revisions"]?[0]?["slots"]?["main"]?["*"]?.ToString();
                if (content == null)
                {
                    throw new AppException("No content found in slot *");
                }
                {
                    return content;
                }
            }

            throw new AppException("No content found in pages");
        }

        private static string ResolveRedirectedPageTitle(string nameVersion, JToken redirects)
        {
            if (!redirects.HasValues)
                throw new AppException($"{nameVersion}: redirect without values; investigate");

            string toValue = redirects[0]["to"].ToString();

            return $"{toValue} [redirected]";
        }

        private JObject GetResponse(string nameVersion)
        {
            string url = $"?action=query&titles={Uri.EscapeDataString(nameVersion)}&prop=pageprops&redirects=1&format=json";

            string json = _httpClient.GetStringAsync(url).Result;
            return JObject.Parse(json);
        }
    }
}
