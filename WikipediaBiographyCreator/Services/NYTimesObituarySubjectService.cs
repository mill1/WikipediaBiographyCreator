using WikipediaBiographyCreator.Extensions;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;
using WikipediaBiographyCreator.Models.NYTimes;

namespace WikipediaBiographyCreator.Services
{
    public class NYTimesObituarySubjectService : INYTimesObituarySubjectService
    {
        private readonly INameVersionService _nameVersionService;

        public NYTimesObituarySubjectService(INameVersionService nameVersionService)
        {
            _nameVersionService = nameVersionService;
        }

        public Subject Resolve(Doc doc)
        {
            // var longestName = obit.Subject.NameVersions.OrderBy(n => n.Length).Last();

            var subjectName = ResolveSubjectName(doc);

            return new Subject
            {
                Name = subjectName,
                NormalizedName = GetNameVersions(subjectName).OrderBy(nv => nv.Length).Last(),
            };
        }

        public List<string> GetNameVersions(string subjectName)
        {
            int i = subjectName.IndexOf(",");

            if (i == -1) // Just one name
                return new List<string> { subjectName.Capitalize() };

            // "BAUMFELD," in request March 1988
            if (!subjectName.Contains(' '))
                return new List<string> { subjectName.Replace(",", "").Capitalize() };

            string surnames = subjectName.Substring(0, i).Capitalize();

            string firstnames = subjectName.Substring(i + 1).Trim();
            firstnames = AdjustFirstNames(firstnames, out string suffix);

            return _nameVersionService.GetNameVersions(firstnames, surnames, suffix);
        }

        private string ResolveSubjectName(Doc doc)
        {
            var person = doc.keywords.FirstOrDefault(k => k.name == "persons");

            if (person != null)
                return person.value;
            else
            {
                int pos = doc.headline.main.IndexOf(',');

                if (pos < 0)
                    return $"Name cannot be resolved. Main: {doc.headline.main.Substring(0, 20)}"; // See feb 1997, article 17c07f7b-b7b9-5c7b-ad28-3e4649d82a08 ; A Whirl Beyond the White House for Stephanopoulos

                return doc.headline.main.Substring(0, pos);
            }
        }

        private string AdjustFirstNames(string firstnames, out string suffix)
        {
            suffix = string.Empty;

            if (firstnames.Length < 3)
                return firstnames;

            // Get the last word of the first names
            string tail = firstnames.Split(' ').Last().ToUpper();

            // Loose any suffixes which are not part of the first name(s)
            // - Jr or Sr
            // - II, III, .. X 
            // - 2D, 3D, 4TH .. 9TH
            // Note : 'I' is kept since it can also mean I. (e.g. SAULS, JOHN I -> John I. Sauls). Same with V and X (OGASAWARA, FRANK X)
            switch (tail)
            {
                case "JR":
                case "SR":
                    suffix = tail.Capitalize() + ".";
                    return firstnames.Substring(0, firstnames.LastIndexOf(' ')).Trim();
                case "II":
                case "III":
                case "IV":
                case "VI":
                case "VII":
                case "VIII":
                case "IX":
                    suffix = tail;
                    return firstnames.Substring(0, firstnames.LastIndexOf(' ')).Trim();

                case "1T":
                case "1ST":
                    suffix = "I";
                    return firstnames.Substring(0, firstnames.LastIndexOf(' ')).Trim();
                case "2D":
                case "2ND":
                    suffix = "II";
                    return firstnames.Substring(0, firstnames.LastIndexOf(' ')).Trim();
                case "3D":
                case "3RD":
                    suffix = "III";
                    return firstnames.Substring(0, firstnames.LastIndexOf(' ')).Trim();
                case "4TH":
                    suffix = "IV";
                    return firstnames.Substring(0, firstnames.LastIndexOf(' ')).Trim();
                case "5TH":
                    suffix = "V";
                    return firstnames.Substring(0, firstnames.LastIndexOf(' ')).Trim();
                case "6TH":
                    suffix = "VI";
                    return firstnames.Substring(0, firstnames.LastIndexOf(' ')).Trim();
                case "7TH":
                    suffix = "VII";
                    return firstnames.Substring(0, firstnames.LastIndexOf(' ')).Trim();
                case "8TH":
                    suffix = "VIII";
                    return firstnames.Substring(0, firstnames.LastIndexOf(' ')).Trim();
                case "9TH":
                    suffix = "IX";
                    return firstnames.Substring(0, firstnames.LastIndexOf(' ')).Trim();
                case "10TH":
                    suffix = "X";
                    return firstnames.Substring(0, firstnames.LastIndexOf(' ')).Trim();
                default:
                    return firstnames.Capitalize();
            }
        }
    }
}
