using WikipediaBiographyCreator.Interfaces;

namespace WikipediaBiographyCreator.Services
{
    public class NameVersionService : INameVersionService
    {
        // TODO: extra version: loose last sur name if multiple surnames: Frances Gershwin Godowsky -> Frances Godowsky
        // TODO: make tests for this service

        public List<string> GetNameVersions(string firstnames, string surnames, string suffix)
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

            for (int i = 0; i < names.Length; i++)
            {
                if (IsNameInitial(names[i]))
                {
                    // Keep the first initial always
                    if (i == 0)
                    {
                        @fixed += $"{names[i]}. ";
                    }
                    else
                    {
                        if (!remove)
                        {
                            @fixed += $"{names[i]}. ";
                        }
                    }
                }
                else
                {
                    @fixed += $"{names[i]} ";
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
