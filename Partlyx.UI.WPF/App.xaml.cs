using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Partlyx.UI.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public App()
        {
            InitializeDI();
        }

        private void InitializeDI()
        {
            var services = new ServiceCollection();

            // DB
            services.AddDbContextFactory<Data.PartlyxDBContext>(opts => opts.UseSqlite("..."));

            // Data
            services.AddTransient<Data.IResourceRepository, Data.ResourceRepository>();

            // Services
            services.AddTransient<Services.IResourceService, Services.ResourceService>();
            services.AddTransient<Services.IRecipeService, Services.RecipeService>();
            services.AddTransient<Services.IRecipeComponentService, Services.RecipeComponentService>();

            services.AddSingleton<CommandDispatcher>();
            services.AddTransient<IServiceProvider, ServiceProvider>();

            // Viewmodels and windows
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();

            Services = services.BuildServiceProvider();
        }
    }

}
