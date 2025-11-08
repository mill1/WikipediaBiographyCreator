using WikipediaBiographyCreator.Models;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IIndependentObitSubjectService : IObitSubjectService<Obituary>, IDoBDoDResolvable
    {
        (DateOnly PublicationDate, string Title, string Description) ExtractMetadata(string html);
    }
}
