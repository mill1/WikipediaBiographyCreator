using Microsoft.Extensions.DependencyInjection;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Services;

namespace WikipediaBiographyCreator
{
    public class Program
    {
        static void Main(string[] args)
        {
            var services = BuildServiceCollection();
            using ServiceProvider sp = services.BuildServiceProvider();
            var service = sp.GetRequiredService<ConsoleUI>();

            service.Run();
        }

        private static IServiceCollection BuildServiceCollection()
        {
            IServiceCollection services = new ServiceCollection();

            services
            .AddSingleton<ConsoleUI>()
            .AddSingleton<IUIActions, UIActions>()
            .AddScoped<IAssemblyService, AssemblyService>()
            .AddScoped<ISignupService, SignupService>();

            return services;
        }
    }
}