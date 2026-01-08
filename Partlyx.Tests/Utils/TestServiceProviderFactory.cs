using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core.Contracts;
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
using Partlyx.Tests.Mocks;
using Partlyx.ViewModels.GlobalNavigations;
using Partlyx.ViewModels.Graph;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.Settings;
using Partlyx.ViewModels.UIObjectViewModels;
using Partlyx.ViewModels.UIServices;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;

namespace Partlyx.Tests.Utils
{
    public static class TestServiceProviderFactory
    {
        public static IServiceProvider Build(Action<IServiceCollection>? configureOverrides = null,
                                             IConfiguration? configuration = null)
        {
            var services = new ServiceCollection();

            services.AddOptions();

            if (configuration != null)
            {
                services.AddSingleton(configuration);
            }

            RegisterAllPotentialServices(services);

            configureOverrides?.Invoke(services);

            return services.BuildServiceProvider(validateScopes: true);
        }

        private static void RegisterAllPotentialServices(IServiceCollection services)
        {
            // Infrastructure
            services.AddTestDataServices();

            services.AddTransient<IWriter, Writer>();
            services.AddTransient<IReader, Reader>();
            services.AddTransient<ILogger, Logger>();

            services.AddTransient<IPartUpdater, PartUpdater>();
            services.AddSingleton<Infrastructure.Events.IEventBus, Infrastructure.Events.EventBus>();

            services.AddSingleton<IApplicationResourceProvider>(new Infrastructure.Data.ApplicationResources.ApplicationResourcesProvider(typeof(UI.Avalonia.App).Assembly));

            services.AddTransient<IPartlyxRepository, PartlyxRepository>();
            services.AddTransient<ISettingsRepository, SettingsRepository>();

            // Services
            services.AddTransient<IServiceProvider, ServiceProvider>();

            services.AddSingleton<IPartsLoaderInitializeService, PartsLoaderInitializeService>();
            services.AddSingleton<IWorkingFileService, WorkingFileService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IServicesResponsibilitySettingsHandler, ServicesResponsibilitySettingsHandler>();

            services.AddTransient<IResourceService, ResourceService>();
            services.AddTransient<IRecipeService, RecipeService>();
            services.AddTransient<IRecipeComponentService, RecipeComponentService>();
            services.AddTransient<IPartsService, PartsService>();

            services.AddTransient<IIconInfoProvider, IconInfoProvider>();

            services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
            services.AddTransient<ICommandFactory, DICommandFactory>();
            services.AddTransient<ICommandServices, CommandServices>();

            services.AddSingleton<ILocalizationService, LocalizationService>();

            InitializeCommands(services);

            // Viewmodels and windows
            services.AddSingleton<MainViewModel>();

            services.AddTransient<PartsTreeViewModel>();
            services.AddTransient<PartsGraphViewModel>();
            services.AddTransient<ItemPropertiesViewModel>();

            services.AddTransient<MenuPanelViewModel>();
            services.AddTransient<MenuPanelFileViewModel>();
            services.AddTransient<MenuPanelEditViewModel>();
            services.AddTransient<MenuPanelSettingsViewModel>();

            services.AddTransient<ComponentCreateViewModel>();

            // Helper viewmodels
            services.AddTransient<SettingsServiceViewModel>();
            services.AddTransient<ApplicationSettingsMenuViewModel>();
            services.AddSingleton<ApplicationSettingsProviderViewModel>();
            services.AddSingleton<IGlobalApplicationSettingsServiceViewModelContainer, ApplicationSettingsServiceViewModelContainer>();

            services.AddTransient<IVMPartsFactory, VMPartsFactory>();
            services.AddSingleton<IVMPartsStore, VMPartsStore>();
            services.AddSingleton<IPartsInitializeServiceViewModel, PartsInitializeServiceViewModel>();
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
            services.AddTransient<IResourceSearchService, ResourceSearchService>();
        }
        private static void InitializeCommands(IServiceCollection services)
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

        private static IServiceCollection AddTestDataServices(this IServiceCollection services)
        {
            // Partlyx DB setting
            services.AddSingleton<IPartlyxDBProvider, TestPartlyxDBProvider>();

            services.AddTransient<IDBLoader, DBLoader>();
            services.AddTransient<IDBSaver, DBSaver>();

            var dbDefaultPath = DirectoryManager.DefaultDBPath;
            var defaultDBConnectionString = @$"DataSource=:memory:";

            services.AddDbContextFactory<PartlyxDBContext>((sp, options) =>
            {
                options.UseSqlite(defaultDBConnectionString);
            });

            // Settings DB setting
            services.AddSingleton<ISettingsDBProvider, TestSettingsDBProvider>();

            var settingsDbDefaultPath = DirectoryManager.DefaultSettingsDBPath;
            var defaultSettingsDBConnectionString = @$"DataSource=:memory:";

            services.AddDbContextFactory<SettingsDBContext>((sp, options) =>
            {
                options.UseSqlite(defaultSettingsDBConnectionString);
            });


            return services;
        }

        public static async void InitializeDatabaseAsync(IServiceProvider services)
        {
            var dbp = services.GetRequiredService<IPartlyxDBProvider>();
            await dbp.InitializeAsync(DirectoryManager.DefaultDBPath);

            var sdbp = services.GetRequiredService<ISettingsDBProvider>();
            await sdbp.InitializeAsync();
        }
    }
}
