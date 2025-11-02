namespace WikipediaBiographyCreator.Interfaces
{
    public interface IIndependentApiService : IApiService
    {
        public void CreateDataSetTmp(List<string> articleUrls);   
    }
}
