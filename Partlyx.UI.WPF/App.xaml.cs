using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure;
using Partlyx.Infrastructure.Data;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.Services.ServiceImplementations;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.UI.WPF.VMImplementations;
using Partlyx.ViewModels;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIObjectViewModels;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using System.Windows;

namespace Partlyx.UI.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            InitializeDI(services);
            Services = services.BuildServiceProvider();

            var mainVM = Services.GetRequiredService<MainViewModel>();
            var window = new MainWindow() { DataContext = mainVM };
            window.Show();

            DirectoryManager.CreatePartlyxFolder();
            InitializeDatabaseAsync();
        }

        private void InitializeDI(IServiceCollection services)
        {
            // Infrastructure
            services = services.AddDataServices();

            services.AddTransient<IWriter, Writer>();
            services.AddTransient<IReader, Reader>();
            services.AddTransient <ILogger, Logger>();

            services.AddTransient<IPartUpdater, PartUpdater>();
            services.AddSingleton<Infrastructure.Events.IEventBus, Infrastructure.Events.EventBus>();


            services.AddTransient<IResourceRepository, ResourceRepository>();

            // Services
            services.AddTransient<IServiceProvider, ServiceProvider>();

            services.AddSingleton<IPartsLoader, PartsLoader>();
            services.AddSingleton<IFileService, FileService>();

            services.AddTransient<IResourceService, ResourceService>();
            services.AddTransient<IRecipeService, RecipeService>();
            services.AddTransient<IRecipeComponentService, RecipeComponentService>();
            services.AddTransient<IPartsService, PartsService>();

            services.AddTransient<IIconInfoProvider, IconInfoProvider>();
            services.AddTransient<IResourceFigureIconService, ResourceFigureIconService>();
            services.AddTransient<IResourceImageIconService, ResourceImageIconService>();

            services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
            services.AddTransient<ICommandFactory, DICommandFactory>();
            services.AddTransient<ICommandServices, CommandServices>();

            InitializeCommands(services);

            // Viewmodels and windows
            services.AddSingleton<MainViewModel>();

            services.AddTransient<ResourceListViewModel>();
            services.AddTransient<RecipeListViewModel>();
            services.AddTransient<RecipeComponentListViewModel>();
            services.AddTransient<PartsTreeViewModel>();

            services.AddTransient<MenuPanelViewModel>();
            services.AddTransient<MenuPanelFileViewModel>();

            services.AddTransient<MainWindow>();

            // Helper viewmodels
            services.AddTransient<IVMPartsFactory, VMPartsFactory>();
            services.AddSingleton<IVMPartsStore, VMPartsStore>();
            services.AddSingleton<IPartsInitializeService, PartsInitializeService>();

            services.AddTransient<ResourceItemViewModel>();
            services.AddTransient<RecipeItemViewModel>();
            services.AddTransient<RecipeComponentItemViewModel>();

            services.AddTransient<IIsolatedSelectedParts, IsolatedSelectedParts>();
            services.AddSingleton<IGlobalSelectedParts, GlobalSelectedParts>();

            services.AddTransient<IResourceItemUiStateService, ResourceItemUiStateService>();
            services.AddTransient<IRecipeItemUiStateService, RecipeItemUiStateService>();
            services.AddTransient<IRecipeComponentItemUiStateService, RecipeComponentItemUiStateService>();

            services.AddTransient<IFileDialogService, WpfFileDialogService>();
            services.AddTransient<IVMFileService, VMFileService>();
            services.AddTransient<INotificationService, WpfNotificationService>();

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

        private async void InitializeDatabaseAsync()
        {
            var dbp = Services.GetRequiredService<IDBProvider>();

            await dbp.InitializeAsync(DirectoryManager.DefaultDBPath);
        }
    }
}
