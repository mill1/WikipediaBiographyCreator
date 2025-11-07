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


        /*
              |
        TMP:  |
              V
        */
        public void CreateDataSetTmp()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var fullPathInput = Path.Combine(baseDir, "Data", "independent-obituaries.json");

            if (!File.Exists(fullPathInput))
                throw new FileNotFoundException($"File not found: {fullPathInput}");

            string json = File.ReadAllText(fullPathInput);

            var archive = JsonConvert.DeserializeObject<string[][]>(json);

            List<string> articleUrls = archive.Select(a => a[2]).Skip(1).ToList();

            var fullPathOutput = Path.Combine(baseDir, "Data", "obituaries-independent-output.csv");
            
            using var writer = new StreamWriter(fullPathOutput, append:false);
            writer.WriteLine("PublicationDate|Title|Name|Name2|WebUrl");

            bool b = true;
            int i = 0;

            foreach (var url in articleUrls)
            {
                if (url.Contains("law-report-"))
                    continue; // miss-archived under obits

                b = !b;
                Thread.Sleep(b ? 388 : 415); // be polite

                switch (url)
                {
                    case "https://www.independent.co.uk/news/obituaries":
                    case "https://www.independent.co.uk/news/obituaries/rss":
                        continue;
                    default:
                        i++;
                        break;
                }

                ConsoleFormatter.WriteInfo($"{i} of {articleUrls.Count} Fetching: {url}");

                // by calling .Result you are synchronously reading the result
                var response = _httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var html = response.Content.ReadAsStringAsync().Result;

                    var (publicationDate, title, excerpt) = ExtractMetadata(html);

                    string name = ResolveObituaryName(title);

                    string name2 = ResolveObituaryName2(excerpt, html);
                    name2 = name2.Replace("\r", " ");
                    name2 = name2.Replace("\n", " ");
                    name2 = name2.Replace("\t", " ");

                    writer.WriteLine($"{publicationDate.ToString("yyyy-MM-dd")}|{title}|{name}|{name2}|{url}");                 
                }
                else
                {
                    continue;
                }
            }
        }

        private string ResolveObituaryName(string title)
        {
            int pos = 0;

            // Example title: "Obituary: John Doe"  ook:  Obituary:Julian Stryjkowski
            if (title.StartsWith("Obituary:", StringComparison.InvariantCultureIgnoreCase))
            {
                pos = title.IndexOf(',');

                if(pos == -1)
                    return title.Substring("Obituary:".Length).Trim();
                else
                    return title.Substring("Obituary:".Length, pos - "Obituary:".Length).Trim();
            }
            // Example title: "OBITUARY : Peng Zhen"
            if (title.StartsWith("OBITUARY :", StringComparison.InvariantCultureIgnoreCase))
            {

                pos = title.IndexOf(",");

                if (pos == -1)
                    return title.Substring("OBITUARY :".Length).Trim();
                else
                    return title.Substring("OBITUARY :".Length, pos - "OBITUARY :".Length).Trim();
            }

            // Example:  Barbara Fiske Calhoun obituary: Wartime cartoonist who dropped out
            title = title.Replace(" obituary", string.Empty, StringComparison.InvariantCultureIgnoreCase);
            title = title.Replace("Remembering ", string.Empty, StringComparison.InvariantCultureIgnoreCase);

            int pos1 = title.IndexOf(':');
            int pos2 = title.IndexOf(',');

            pos = Math.Min(
                pos1 == -1 ? int.MaxValue : pos1,
                pos2 == -1 ? int.MaxValue : pos2
            );

            if (pos1 == -1 && pos2 == -1)
                // Example title: "John Kramer"
                return title;
            else
                // Example title: "William A Fraker: Celebrated cinematographer"
                return title.Substring(0, Math.Min(pos, 127));
        }

        private string ResolveObituaryName2(string excerpt, string html)
        {
            int pos = excerpt.IndexOf(',');

            if (pos == -1)
            {
                var bodyText = ExtractBodyText(html);

                int pos1 = bodyText.IndexOf(",");
                int pos2 = bodyText.IndexOf(" was ");

                pos = Math.Min(
                    pos1 == -1 ? int.MaxValue : pos1,
                    pos2 == -1 ? int.MaxValue : pos2
                );

                if (pos1 == -1 && pos2 == -1)
                    return "Name cannot be resolved.";
                else
                    return bodyText.Substring(0, Math.Min(pos, 127));
            }
            else
            {
                // Example excerpt: Ion Voicu, the elder statesman of Romanian violinists
                return excerpt.Substring(0, pos);
            }
        }      

        public static (DateOnly PublicationDate, string Title, string Excerpt) ExtractMetadata(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Helper to get meta content by either "property" or "name"
            string? GetMetaContent(string attrName, string attrValue)
            {
                return doc.DocumentNode
                    .SelectSingleNode($"//meta[@{attrName}='{attrValue}']")
                    ?.GetAttributeValue("content", null);
            }

            // Try all possible date sources
            string? dateStr = 
                GetMetaContent("property", "article:published_time") ??
                GetMetaContent("property", "og:updated_time") ??
                GetMetaContent("property", "date");

            // Title (almost always under og:title)
            string? title =
                GetMetaContent("property", "og:title") ??
                doc.DocumentNode.SelectSingleNode("//title")?.InnerText;

            // --- description/excerpt ---
            string? description =
                GetMetaContent("name", "description") ??
                GetMetaContent("property", "og:description");

            string? excerpt = null;
            if (!string.IsNullOrWhiteSpace(description))
            {
                description = description.Trim();
                excerpt = description.Length > 127 ? description[..127] : description;
            }

            // Parse ISO date safely to DateOnly
            DateOnly publicationDate = DateOnly.FromDateTime(
                DateTime.Parse(dateStr!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
            );

            return (publicationDate, title ?? string.Empty, excerpt ?? string.Empty);
        }
    }
}
