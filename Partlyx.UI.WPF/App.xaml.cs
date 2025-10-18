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
using Partlyx.UI.WPF.DragAndDrop;
using Partlyx.UI.WPF.VMImplementations;
using Partlyx.ViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIObjectViewModels;
using Partlyx.ViewModels.UIServices;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using System.Windows;
using System.Windows.Threading;

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


            services.AddTransient<IPartlyxRepository, PartlyxRepository>();

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

            services.AddTransient<PartsTreeViewModel>();
            services.AddTransient<PartsGraphViewModel>();
            services.AddTransient<ItemPropertiesViewModel>();

            services.AddTransient<MenuPanelViewModel>();
            services.AddTransient<MenuPanelFileViewModel>();

            services.AddTransient<ComponentCreateViewModel>();

            services.AddTransient<MainWindow>();

            // Other view classes
            services.AddTransient<PartsTreeDropHandler>();

            // Helper viewmodels
            services.AddTransient<IVMPartsFactory, VMPartsFactory>();
            services.AddSingleton<IVMPartsStore, VMPartsStore>();
            services.AddSingleton<IPartsInitializeService, PartsInitializeService>();
            services.AddSingleton<IVMPartsStoreCleaner, VMPartsStoreCleaner>();
            services.AddSingleton<IGuidLinkedPartFactory, GuidLinkedPartFactory>();
            services.AddSingleton<ILinkedPartsManager, LinkedPartsManager>();

            services.AddTransient<ResourceViewModel>();
            services.AddTransient<RecipeViewModel>();
            services.AddTransient<RecipeComponentViewModel>();

            services.AddTransient<ResourceServiceViewModel>();
            services.AddTransient<RecipeServiceViewModel>();
            services.AddTransient<RecipeComponentServiceViewModel>();
            services.AddTransient<PartsServiceViewModel>();

            services.AddTransient<PanAndZoomControllerViewModel>();

            services.AddTransient<IIsolatedSelectedParts, IsolatedSelectedParts>();
            services.AddSingleton<IGlobalSelectedParts, GlobalSelectedParts>();
            services.AddTransient<IIsolatedFocusedPart, IsolatedFocusedPart>();
            services.AddSingleton<IGlobalFocusedPart, GlobalFocusedPart>();
            services.AddTransient<IIsolatedResourcesVMContainer, ResourcesVMContainer>();
            services.AddSingleton<IGlobalResourcesVMContainer, ResourcesVMContainer>();

            services.AddTransient<IResourceItemUiStateService, ResourceItemUiStateService>();
            services.AddTransient<IRecipeItemUiStateService, RecipeItemUiStateService>();
            services.AddTransient<IRecipeComponentItemUiStateService, RecipeComponentItemUiStateService>();
            services.AddTransient<IResourceSearchService, ResourceSearchService>();

            services.AddTransient<IFileDialogService, WpfFileDialogService>();
            services.AddTransient<IVMFileService, VMFileService>();
            services.AddTransient<INotificationService, WpfNotificationService>();
            services.AddTransient<IDialogService, DialogService>();

            services.AddSingleton<IDispatcherInvoker, WPFDispatcherInvoker>(sp => new WPFDispatcherInvoker(Dispatcher.CurrentDispatcher));

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
