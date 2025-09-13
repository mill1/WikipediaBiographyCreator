using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Services;

/*
    Stap 1: obits bepalen Guardian. Model: suject name, YoB, YoD
    Stap 2: obits bepalen NYTimes. Model: subject name
    Stap 3: Gemeenschappelijke subject names bepalen    
    Stap 4: Voor elke subject name bestaan op Wikipedia bepalen
    Stap 5: Indien nee; Wikipedia bio creëren met Guardian en NYTimes als referentie
 */

namespace WikipediaBiographyCreator
{
    public class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var services = BuildServiceCollection(configuration);
            using ServiceProvider sp = services.BuildServiceProvider();
            var service = sp.GetRequiredService<ConsoleUI>();

            service.Run();
        }

        private static IServiceCollection BuildServiceCollection(IConfiguration config)
        {
            IServiceCollection services = new ServiceCollection();

            services
            .AddSingleton<IConfiguration>(config)
            .AddSingleton<ConsoleUI>()
            .AddSingleton<IUIActions, UIActions>()
            .AddScoped<IAssemblyService, AssemblyService>()
            .AddScoped<INYTimesApiService, NYTimesApiService>()
            .AddScoped<INYTimesObituarySubjectService, NYTimesObituarySubjectService>()
            .AddScoped<ISignupService, SignupService>()
            .AddHttpClient<INYTimesApiService, NYTimesApiService>();

            return services;
        }
    }
}