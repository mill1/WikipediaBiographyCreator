using System.Reflection;

namespace WikipediaBiographyCreator.Interfaces
{
    public interface IAssemblyService
    {
        AssemblyName GetAssemblyName();
        string GetAssemblyValue(string propertyName, AssemblyName assemblyName);
    }
}
