using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure.Data;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.Services.Commands.ResourceCommonCommands;
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
            services.AddDbContextFactory<PartlyxDBContext>(opts => opts.UseSqlite("..."));

            // Data
            services.AddTransient<IResourceRepository, ResourceRepository>();

            // Services
            services.AddTransient<Services.IResourceService, Services.ResourceService>();
            services.AddTransient<Services.IRecipeService, Services.RecipeService>();
            services.AddTransient<Services.IRecipeComponentService, Services.RecipeComponentService>();

            services.AddSingleton<Services.Commands.CommandDispatcher>();
            services.AddTransient<IServiceProvider, ServiceProvider>();
            services.AddTransient<ICommandFactory, DICommandFactory>();

            InitializeCommands(services);

            // Viewmodels and windows
            services.AddTransient<MainViewModel>();

            services.AddTransient<ResourceListViewModel>();
            services.AddTransient<RecipeListViewModel>();
            services.AddTransient<RecipeComponentsViewModel>();
            services.AddTransient<PartsTreeViewModel>();

            services.AddTransient<MainWindow>();

            Services = services.BuildServiceProvider();
        }

        private void InitializeCommands(ServiceCollection services)
        {
            // Resource commands
            services.AddTransient<CreateResourceCommand>();
            services.AddTransient<DeleteResourceCommand>();
            services.AddTransient<DuplicateResourceCommand>();
            services.AddTransient<SetDefaultRecipeToResourceCommand>();
            services.AddTransient<SetNameToResourceCommand>();

            // Recipe commands
            services.AddTransient<CreateRecipeCommand>();
            services.AddTransient<DeleteRecipeCommand>();
            services.AddTransient<DuplicateRecipeCommand>();
            services.AddTransient<SetRecipeCraftAmountCommand>();

            // Recipe component commands
            services.AddTransient<CreateRecipeComponentCommand>();
            services.AddTransient<DeleteRecipeComponentCommand>();
            services.AddTransient<DuplicateRecipeComponentCommand>();
            services.AddTransient<SetRecipeComponentQuantityCommand>();
            services.AddTransient<SetRecipeComponentResourceCommand>();
        }
    }

}
