using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure;
using Partlyx.Infrastructure.Data;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.ViewModels;
using Partlyx.ViewModels.PartsViewModels;
using System.Configuration;
using System.Data;
using System.Windows;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.Services.ServiceImplementations;
using Partlyx.ViewModels.UIObjectViewModels;
using Partlyx.ViewModels.UIServices.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;

namespace Partlyx.UI.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            InitializeDI(services);
            Services = services.BuildServiceProvider();

            InitializeDatabase();
        }

        private void InitializeDI(IServiceCollection services)
        {
            // DB
            services.AddSingleton<SqliteConnection>(sp =>
            {
                var conn = new SqliteConnection("DataSource=:memory:;Cache=Shared");
                conn.Open();
                return conn;
            });

            services.AddDbContextFactory<PartlyxDBContext>((sp, opt) =>
            {
                var conn = sp.GetRequiredService<SqliteConnection>();
                opt.UseSqlite(conn);
            });

            // Infrastructure
            services.AddTransient<IPartUpdater, PartUpdater>();
            services.AddSingleton<Infrastructure.Events.IEventBus, Infrastructure.Events.EventBus>();

            services.AddTransient<IResourceRepository, ResourceRepository>();


            // Services
            services.AddTransient<Services.Commands.ICommandDispatcher, Services.Commands.CommandDispatcher>();
            services.AddTransient<IResourceService, ResourceService>();
            services.AddTransient<IRecipeService, RecipeService>();
            services.AddTransient<IRecipeComponentService, RecipeComponentService>();
            services.AddTransient<IPartsService, PartsService>();

            services.AddTransient<IIconInfoProvider, IconInfoProvider>();
            services.AddTransient<IResourceFigureIconService, ResourceFigureIconService>();
            services.AddTransient<IResourceImageIconService, ResourceImageIconService>();

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

            // Helper viewmodels
            services.AddTransient<IVMPartsFactory, VMPartsFactory>();

            services.AddTransient<ResourceItemViewModel>();
            services.AddTransient<RecipeItemViewModel>();
            services.AddTransient<RecipeComponentItemViewModel>();

            services.AddSingleton<IVMPartsStore, VMPartsStore>();

            services.AddTransient<IResourceItemUiStateService, ResourceItemUiStateService>();

            Services = services.BuildServiceProvider();
        }

        private void InitializeCommands(IServiceCollection services)
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

        private void InitializeDatabase()
        {
            using var scope = Services.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PartlyxDBContext>>();
            using var ctx = factory.CreateDbContext();

            ctx.Database.EnsureCreated();
        }
    }
}
