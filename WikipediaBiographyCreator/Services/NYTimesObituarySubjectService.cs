using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Services
{
    public class NYTimesObituarySubjectService : INYTimesObituarySubjectService
    {
        public Subject Resolve(Doc doc)
        {
            return new Subject
            {
                Name = ResolveSubjectName(doc),
                NameVersions = ResolveNameVersions(doc),
                YearOfBirth = -1, // Not available in the API response
                YearOfDeath = -1, // Not available in the API response
            };
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
                    return "Unknown name (NYTimes)"; // See feb 1997, article 17c07f7b-b7b9-5c7b-ad28-3e4649d82a08 ; A Whirl Beyond the White House for Stephanopoulos

                return doc.headline.main.Substring(0, pos);
            }
        }

        public List<string> ResolveNameVersions(Doc doc)
        {
            string name = ResolveSubjectName(doc);

            int i = name.IndexOf(",");

            if (i == -1) // Just one name
                return new List<string> { Capitalize(name) };

            // "BAUMFELD," in request March 1988
            if (!name.Contains(' '))
                return new List<string> { Capitalize(name.Replace(",", "")) };

            string surnames = Capitalize(name.Substring(0, i));

            string firstnames = name.Substring(i + 1).Trim();
            firstnames = AdjustFirstNames(firstnames, out string suffix);

            return GetNameVersions(firstnames, surnames, suffix);
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
            // Note : I is kept since it can also mean I. (e.g. SAULS, JOHN I -> John I. Sauls). Same with V and X (OGASAWARA, FRANK X)
            switch (tail)
            {
                case "JR":
                case "SR":
                    suffix = Capitalize(tail) + ".";
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
                    return Capitalize(firstnames);
            }
        }

        private string Capitalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            // Normalize spaces
            value = value.Trim();

            // Split words, ignore multiple spaces
            var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Capitalize each word
            var capitalizedWords = words.Select(w =>
                char.ToUpper(w[0]) + w.Substring(1).ToLower()
            );

            return string.Join(' ', capitalizedWords);
        }


        private List<string> GetNameVersions(string firstnames, string surnames, string suffix)
        {
            if (string.IsNullOrEmpty(suffix))
            {
                if (HasNameInitial(firstnames))
                    return GetNameVersionsInitialsNoSuffix(firstnames, surnames);
                else
                    return new List<string> { $"{firstnames} {surnames}" };
            }
            else
            {
                if (HasNameInitial(firstnames))
                    return GetNameVersionsInitials(firstnames, surnames, suffix);
                else
                    return GetNameVersionsNoInitials(firstnames, surnames, suffix);
            }
        }

        private List<string> GetNameVersionsNoInitials(string firstnames, string surnames, string suffix)
        {
            return new List<string>
            {
                $"{firstnames} {surnames} {suffix}",
                $"{firstnames} {surnames}"
            };
        }

        private List<string> GetNameVersionsInitials(string firstnames, string surnames, string suffix)
        {
            return new List<string>
            {
                $"{FixNameInitials(firstnames, false)} {surnames} {suffix}",
                $"{FixNameInitials(firstnames, true)} {surnames} {suffix}",
                $"{FixNameInitials(firstnames, false)} {surnames}",
                $"{FixNameInitials(firstnames, true)} {surnames}"
            };
        }

        private List<string> GetNameVersionsInitialsNoSuffix(string firstnames, string surnames)
        {
            return new List<string>{
                $"{FixNameInitials(firstnames, false)} {surnames}",
                $"{FixNameInitials(firstnames, true)} {surnames}" };
        }

        private string FixNameInitials(string firstnames, bool remove)
        {
            string @fixed = "";

            var names = firstnames.Split(" ");

            foreach (string name in names)
            {
                if (IsNameInitial(name))
                {
                    // Keep the first initial always
                    if (name == names[0])
                    {
                        @fixed += $"{name}. ";
                    }
                    else
                    {
                        if (!remove)
                        {
                            @fixed += $"{name}. ";
                        }
                    }
                }
                else
                {
                    @fixed += $"{name} ";
                }
            }
            return @fixed.Trim();
        }

        private bool HasNameInitial(string firstnames)
        {
            foreach (string name in firstnames.Split(" "))
                if (IsNameInitial(name))
                    return true;

            return false;
        }

        private bool IsNameInitial(string name)
        {
            if (name.Length == 1 && name.Equals(name.ToUpper()))
                return true;

            return false;
        }
    }
}
