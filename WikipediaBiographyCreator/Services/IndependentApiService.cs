using Microsoft.Extensions.Configuration;
using System.Globalization;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Services
{
    public class IndependentApiService : IIndependentApiService
    {
        private readonly HttpClient _httpClient;

        public IndependentApiService(
            HttpClient httpClient,
            IAssemblyService assemblyService)
        {
            _httpClient = httpClient;

            var assemblyName = assemblyService.GetAssemblyName();
            string agent = $"{assemblyName.Name} v.{assemblyName.Version}";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", agent);
        }

        public void CreateDataSetTmp(List<string> articleUrls)
        {
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "obituaries-independent.csv");
            
            using var writer = new StreamWriter(fullPath, append:false);
            writer.WriteLine("Id;PublicationDate;Title;Name;NormalizedName;WebUrl");

            bool b = true;

            foreach (var url in articleUrls)
            {
                b = !b;
                Thread.Sleep(b ? 400 : 1300); // be polite

                ConsoleFormatter.WriteInfo($"Fetching: {url}");

                // by calling .Result you are synchronously reading the result
                var response = _httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;

                    string id = Guid.NewGuid().ToString();
                    // publication date is the current date format: yyyy-MM-dd
                    string publicationDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
                    string title = "Obituary " + id;
                    string name = id;
                    string normalizedName = id;
                    string webUrl = url;

                    writer.WriteLine($"{id};{publicationDate};{title};{name};{normalizedName};{webUrl}");
                }
                else
                {
                    continue;
                }
            }
        }

        public List<Obituary> ResolveObituariesOfMonth(int year, int monthId)
        {
            // if( not loaded..
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "obituaries-independent.csv");

            var obituaries = ReadFromCsv(fullPath);

            return obituaries
                .Where(o => o.PublicationDate.Year == year &&
                            o.PublicationDate.Month == monthId)
                .ToList();
        }

        private static List<Obituary> ReadFromCsv(string path)
        {
            var lines = File.ReadAllLines(path)
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .Skip(1); // skip header line

            var list = new List<Obituary>();

            foreach (var line in lines)
            {
                var fields = line.Split(';');

                if (fields.Length < 6)
                    continue; // skip malformed lines

                var obituary = new Obituary
                {
                    Id = fields[0],

                    // parse "yyyy-MM-dd" safely into DateOnly
                    PublicationDate = DateOnly.ParseExact(
                        fields[1],
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture
                    ),

                    Title = fields[2],

                    Subject = new Subject
                    {
                        Name = fields[3],
                        NormalizedName = fields[4]
                    },

                    WebUrl = fields[5]
                };

                list.Add(obituary);
            }

            return list;
        }
    }
}
