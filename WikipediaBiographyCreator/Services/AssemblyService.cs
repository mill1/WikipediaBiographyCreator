using System.Reflection;
using WikipediaBiographyCreator.Interfaces;

namespace WikipediaBiographyCreator.Services
{
    public class AssemblyService : IAssemblyService
    {
        public readonly Assembly ExecutingAssembly;

        public AssemblyService()
        {
            ExecutingAssembly = Assembly.GetExecutingAssembly();
        }

        public AssemblyName GetAssemblyName()
        {
            return ExecutingAssembly.GetName();
        }

        public string GetAssemblyValue(string propertyName, AssemblyName assemblyName)
        {
            return GetAssemblyPropertyValue(assemblyName, propertyName).ToString();
        }

        private static string GetAssemblyPropertyValue(AssemblyName assemblyName, string propertyName)
        {
            return assemblyName.GetType().GetProperty(propertyName).GetValue(assemblyName, null).ToString();
        }
    }
}
