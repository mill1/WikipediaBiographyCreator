using WikipediaBiographyCreator.Models.Guardian;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IGuardianObituarySubjectService : IObituarySubjectService<Result>
    {
        (int YearOfBirth, int YearOfDeath) ResolveYoBAndYoD(string obituaryText);
    }
}
