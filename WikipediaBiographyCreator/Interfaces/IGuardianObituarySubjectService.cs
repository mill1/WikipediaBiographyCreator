using WikipediaBiographyCreator.Models.Guardian;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IGuardianObituarySubjectService : IObituarySubjectService<Result>
    {
        (DateOnly DateOfBirth, DateOnly DateOfDeath) ResolveDoBAndDoD(string obituaryText);
    }
}
