using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Globalization;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Exceptions;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Services
{    
    public class IndependentApiService : IIndependentApiService
    {
        private readonly HttpClient _httpClient;
        private List<Obituary> obituaries;

        public IndependentApiService(
            HttpClient httpClient,
            IAssemblyService assemblyService)
        {
            _httpClient = httpClient;

            var assemblyName = assemblyService.GetAssemblyName();
            string agent = $"{assemblyName.Name} v.{assemblyName.Version}";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", agent);
        }

        public List<Obituary> ResolveObituariesOfMonth(int year, int monthId)
        {
            if (obituaries == null)
            {
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "obituaries-independent.csv");
                obituaries = ReadFromCsv(fullPath);
            }

            return obituaries
                .Where(o => o.PublicationDate.Year == year &&
                            o.PublicationDate.Month == monthId)
                .ToList();
        }

        private static List<Obituary> ReadFromCsv(string path)
        {
            var lines = File.ReadAllLines(path)
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .Skip(1); // skip header

            var list = new List<Obituary>();

            foreach (var line in lines)
            {
                var fields = line.Split(';');

                var obituary = new Obituary
                {
                    Id = fields[0],
                    Source = "Independent",
                    PublicationDate = DateOnly.ParseExact(fields[1], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Title = fields[2],
                    Subject = new Subject
                    {
                        Name = fields[3],
                        NormalizedName = fields[4]
                    },
                    WebUrl = fields[5],
                    FullTextUrl = fields[5]
                };

                list.Add(obituary);
            }
            return list;
        }

        public string GetObituaryText(string apiUrl, string subjectName)
        {
            var response = _httpClient.GetAsync(apiUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                var html = response.Content.ReadAsStringAsync().Result;

                return ExtractBodyText(html);
            }
            else
            {
                throw new AppException($"Response status: {response.StatusCode}. Subject: {subjectName}");
            }
        }

        private static string ExtractBodyText(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Select all paragraph tags under the article content
            var paragraphs = doc.DocumentNode
                .SelectNodes("//article[@id='articleContent']//p");

            if (paragraphs == null || paragraphs.Count == 0)
                return null;

            // Filter out empty or ad/utility paragraphs
            var cleanParagraphs = paragraphs
                .Select(p => p.InnerText.Trim())
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Where(text =>
                    !text.Contains("email", StringComparison.OrdinalIgnoreCase) &&
                    !text.Contains("newsletter", StringComparison.OrdinalIgnoreCase) &&
                    !text.Contains("Sign up", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Join into one block of text
            var fullText = string.Join("\n\n", cleanParagraphs);

            return fullText;
        }
    }
}
