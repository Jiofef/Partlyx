using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core.Contracts;
using Partlyx.Core.Technical;
using Partlyx.Infrastructure;
using Partlyx.Infrastructure.Data;
using Partlyx.Infrastructure.Data.ApplicationResources;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.Services.ServiceImplementations;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.UI.Avalonia.Resources;
using Partlyx.UI.Avalonia.VMImplementations;
using Partlyx.ViewModels.GlobalNavigations;
using Partlyx.ViewModels.Graph;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.Settings;
using Partlyx.ViewModels.UIObjectViewModels;
using Partlyx.ViewModels.UIServices;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using Partlyx.ViewModels.UIStates;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        public static ILocalizationService LocService { get; private set; }

        public App()
        {
            var resourceProvider = new ApplicationResourcesProvider(typeof(App).Assembly);
            LocService = new LocalizationService(resourceProvider);
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var services = new ServiceCollection();
            InitializeDI(services);
            Services = services.BuildServiceProvider();

            var mainVM = Services.GetRequiredService<MainViewModel>();
            var window = new MainWindow() { DataContext = mainVM };
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = window;
            }

            DirectoryManager.CreatePartlyxFolder();
            LoadJsonSavesAsync();
            InitializeDatabaseAsync();

            if (DevStaticConfig.ENABLE_VISUAL_UNHANDLED_EXCEPTIONS)
            {
                Dispatcher.UIThread.UnhandledException += (s, e) =>
                {
                    e.Handled = true;

                    ShowError(e.Exception);
                };
            }

            base.OnFrameworkInitializationCompleted();

            _ = mainVM.OnInitializeFinished();
        }


        private void InitializeDI(IServiceCollection services)
        {
            // Infrastructure
            services = services.AddDataServices();

            services.AddTransient<IWriter, Writer>();
            services.AddTransient<IReader, Reader>();
            services.AddTransient<ILogger, Logger>();

            services.AddTransient<IPartUpdater, PartUpdater>();
            services.AddSingleton<Infrastructure.Events.IEventBus, Infrastructure.Events.EventBus>();
            services.AddSingleton<Infrastructure.Events.IRoutedEventBus, Infrastructure.Events.RoutedEventBus>();

            services.AddSingleton<IApplicationResourceProvider>(new Infrastructure.Data.ApplicationResources.ApplicationResourcesProvider(typeof(App).Assembly));

            services.AddTransient<IPartlyxRepository, PartlyxRepository>();
            services.AddTransient<ISettingsRepository, SettingsRepository>();

            // Services
            services.AddTransient<IServiceProvider, ServiceProvider>();

            services.AddSingleton<IPartsLoaderInitializeService, PartsLoaderInitializeService>();
            services.AddSingleton<IImagesLoaderInitializeService, ImagesLoaderInitializeService>();
            services.AddSingleton<IWorkingFileService, WorkingFileService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IServicesResponsibilitySettingsHandler, ServicesResponsibilitySettingsHandler>();
            services.AddSingleton<IJsonSavesService, JsonSavesService>();

            services.AddTransient<IResourceService, ResourceService>();
            services.AddTransient<IRecipeService, RecipeService>();
            services.AddTransient<IRecipeComponentService, RecipeComponentService>();
            services.AddTransient<IPartsService, PartsService>();

            services.AddTransient<IIconInfoProvider, IconInfoProvider>();

            services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
            services.AddTransient<ICommandFactory, DICommandFactory>();
            services.AddTransient<ICommandServices, CommandServices>();

            services.AddTransient<IPartlyxImageService, PartlyxImageService>();

            services.AddSingleton(LocService);

            InitializeCommands(services);

            // Viewmodels and windows
            services.AddSingleton<MainViewModel>();

            services.AddTransient<PartsTreeViewModel>();
            services.AddTransient<PartsTreeResourcesViewModel>();
            services.AddTransient<PartsTreeRecipesViewModel>();
            services.AddTransient<PartsGraphViewModel>();
            services.AddTransient<ItemPropertiesViewModel>();

            services.AddTransient<MenuPanelViewModel>();
            services.AddTransient<MenuPanelFileViewModel>();
            services.AddTransient<MenuPanelEditViewModel>();
            services.AddTransient<MenuPanelProjectViewModel>();
            services.AddTransient<MenuPanelSettingsViewModel>();
            services.AddTransient<MenuPanelHelpViewModel>();

            services.AddTransient<ComponentCreateViewModel>();

            services.AddTransient<IconsMenuViewModel>();

            services.AddTransient<AboutUsWindowViewModel>();
            services.AddTransient<HelpWindowViewModel>();

            services.AddTransient<IconsMenuViewModel>();

            services.AddTransient<MainWindow>();

            // Helper viewmodels
            services.AddTransient<SettingsServiceViewModel>();
            services.AddTransient<ApplicationSettingsMenuViewModel>();
            services.AddSingleton<ApplicationSettingsProviderViewModel>();
            services.AddSingleton<IGlobalApplicationSettingsServiceViewModelContainer, ApplicationSettingsServiceViewModelContainer>();

            services.AddTransient<IVMPartsFactory, VMPartsFactory>();
            services.AddSingleton<IVMPartsStore, VMPartsStore>();
            services.AddSingleton<VMComponentsGraphs>();
            services.AddSingleton<IPartsInitializeServiceViewModel, PartsInitializeServiceViewModel>();
            services.AddSingleton<IViewModelStorePartsEventRouter, ViewModelStorePartsEventRouter>();
            services.AddSingleton<IVMPartsStoreCleaner, VMPartsStoreCleaner>();
            services.AddSingleton<IGuidLinkedPartFactory, GuidLinkedPartFactory>();
            services.AddSingleton<ILinkedPartsManager, LinkedPartsManager>();

            services.AddSingleton<IMainWindowController, MainWindowController>();
            services.AddTransient<MainWindowNameController>();

            services.AddTransient<IGraphTreeBuilderViewModel, MSAGLGraphBuilderViewModel>();
            services.AddTransient<PartsGraphBuilderViewModel>();

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
            services.AddTransient<IIsolatedFocusedElementContainer, IsolatedFocusedPart>();
            services.AddSingleton<IGlobalFocusedElementContainer, GlobalFocusedPart>();
            services.AddTransient<IIsolatedResourcesVMContainer, ResourcesVMContainer>();
            services.AddSingleton<IGlobalResourcesVMContainer, ResourcesVMContainer>();
            services.AddTransient<IIsolatedRecipesVMContainer, RecipesVMContainer>();
            services.AddSingleton<IGlobalRecipesVMContainer, RecipesVMContainer>();

            services.AddSingleton<PartsGlobalNavigations>();

            services.AddTransient<IResourceItemUiStateService, ResourceItemUiStateService>();
            services.AddTransient<IRecipeItemUiStateService, RecipeItemUiStateService>();
            services.AddTransient<IRecipeComponentItemUiStateService, RecipeComponentItemUiStateService>();
            services.AddTransient<IComponentPathUiStateService, ComponentPathUiStateService>();
            services.AddTransient<IResourceSearchService, ResourceSearchService>();
            services.AddTransient<IRecipeSearchService, RecipeSearchService>();

            services.AddTransient<IconServiceViewModel>();
            services.AddTransient<ImageViewModel>();
            services.AddTransient<ImageUiItemStateViewModel>();
            services.AddSingleton<IImagesStoreViewModel, ImagesStoreViewModel>();
            services.AddSingleton<ImagesInitializeServiceViewModel>();
            services.AddTransient<ImageFactoryViewModel>();
            services.AddSingleton<ImageUiItemStateFactoryViewModel>();
            services.AddTransient<InheritedIconHelperServiceViewModel>();

            services.AddTransient<IFileDialogService, AvaloniaFileDialogService>();
            services.AddTransient<IVMFileService, VMFileService>();
            services.AddTransient<INotificationService, AvaloniaNotificationService>();
            services.AddTransient<IDialogService, DialogService>();
            services.AddSingleton<IIconVectorCatalog, MaterialAvaloniaVectorIconCatalog>();
            services.AddTransient<ITimerService, AvaloniaTimerService>();

            services.AddSingleton<IDispatcherInvoker, AvaloniaDispatcherInvoker>(sp => new AvaloniaDispatcherInvoker());

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
            // Recipe component commands
            services.AddTransient<CreateRecipeComponentCommand>();
            services.AddTransient<DeleteRecipeComponentCommand>();
            services.AddTransient<DuplicateRecipeComponentCommand>();
            services.AddTransient<SetRecipeComponentQuantityCommand>();
            services.AddTransient<SetRecipeComponentResourceCommand>();
        }

        private async void LoadJsonSavesAsync()
        {
            var jss = Services.GetRequiredService<IJsonSavesService>();
            await jss.LoadGlobalSchemesAsync();
        }

        private async void InitializeDatabaseAsync()
        {
            var dbp = Services.GetRequiredService<IPartlyxDBProvider>();
            await dbp.InitializeAsync(DirectoryManager.DefaultDBPath);

            var sdbp = Services.GetRequiredService<ISettingsDBProvider>();
            await sdbp.InitializeAsync();
        }

        private void MaterialIcon_ActualThemeVariantChanged(object? sender, EventArgs e)
        {
        }

        void ShowError(Exception ex)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var window = new Window
                {
                    Title = LocService.Get("Error"),
                    Content = new TextBox
                    {
                        Text = ex.ToString(),
                        IsReadOnly = true
                    },
                    Width = 600,
                    Height = 400
                };

                window.Show();
            });
        }

    }
}
