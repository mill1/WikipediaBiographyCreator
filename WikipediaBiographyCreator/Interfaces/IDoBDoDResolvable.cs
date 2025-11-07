namespace WikipediaBiographyCreator.Interfaces
{
    public interface IDoBDoDResolvable
    {
        (DateOnly DateOfBirth, DateOnly DateOfDeath) ResolveDoBAndDoD(string obituaryText);
    }
}
