using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WikipediaBiographyCreator.Console;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Services;

namespace WikipediaBiographyCreator
{
    /*
       Results can be found here: https://en.wikipedia.org/wiki/User:Mill_1/Project_Finding_The_Forgotten_Few
     */
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
            .AddScoped<IWikipediaApiService, WikipediaApiService>()
            .AddScoped<IWikipediaBiographyService, WikipediaBiographyService>()
            .AddScoped<INameVersionService, NameVersionService>()
            .AddScoped<IDisambiguationResolver, DisambiguationResolver>()
            .AddScoped<IGuardianApiService, GuardianApiService>()
            .AddScoped<IGuardianObituarySubjectService, GuardianObituarySubjectService>()
            .AddScoped<INYTimesApiService, NYTimesApiService>()
            .AddScoped<INYTimesObituarySubjectService, NYTimesObituarySubjectService>()
            .AddScoped<IIndependentApiService, IndependentApiService>(); // TODO test caching

            services.AddHttpClient<IGuardianApiService, GuardianApiService>();
            services.AddHttpClient<INYTimesApiService, NYTimesApiService>();
            services.AddHttpClient<IIndependentApiService, IndependentApiService>();

            return services;
        }
    }
}