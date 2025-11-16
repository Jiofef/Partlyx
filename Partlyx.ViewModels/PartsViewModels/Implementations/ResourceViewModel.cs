using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.GlobalNavigations;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using Partlyx.ViewModels.UIStates;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public partial class ResourceViewModel : UpdatableViewModel<ResourceDto>, IVMPart
    {
        // Services
        private readonly IVMPartsStore _store;
        private readonly IVMPartsFactory _partsFactory;
        private readonly IResourceItemUiStateService _uiStateService;
        private readonly ICommandServices _commands;
        private readonly ILinkedPartsManager _linkedPartsManager;
        public PartsServiceViewModel Services { get; }

        // Events
        private readonly IEventBus _bus;
        private readonly IDisposable _updatedSubscription;
        private readonly IDisposable _childAddSubscription;
        private readonly IDisposable _childRemoveSubscription;
        private readonly IDisposable _childMoveSubscription;

        private ResourceViewModel() { }
        public ResourceViewModel(ResourceDto dto, PartsServiceViewModel service, PartsGlobalNavigations nav, IVMPartsStore store, IVMPartsFactory partsFactory,
            IEventBus bus, IResourceItemUiStateService uiStateS, ICommandServices cs, ILinkedPartsManager lpm)
        {
            Uid = dto.Uid;

            // Services
            Services = service;
            GlobalNavigations = nav;
            _store = store;
            _partsFactory = partsFactory;
            _bus = bus;
            _uiStateService = uiStateS;
            _commands = cs;
            _linkedPartsManager = lpm;

            // Info
            _name = dto.Name;
            if (dto.DefaultRecipeUid is Guid dRecipeUid)
                _defaultRecipe = _linkedPartsManager.CreateAndRegisterLinkedRecipeVM(dRecipeUid);

            _icon = new IconViewModel(dto.Icon);

            foreach (var recipe in dto.Recipes)
            {
                var vm = _partsFactory.GetOrCreateRecipeVM(recipe);
                _recipes.Add(vm);
            }

            // Info updating binding
            _updatedSubscription = bus.Subscribe<ResourceUpdatedEvent>(OnResourceUpdated, true);
            _childAddSubscription = bus.Subscribe<RecipeCreatedEvent>(OnRecipeCreated, true);
            _childRemoveSubscription = bus.Subscribe<RecipeDeletedEvent>(OnRecipeDeleted, true);
            _childMoveSubscription = bus.Subscribe<RecipeMovedEvent>(OnRecipeMoved, true);
        }

        // Resource info
        public PartTypeEnumVM PartType { get => PartTypeEnumVM.Resource; }

        public Guid Uid { get; }

        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private ObservableCollection<RecipeViewModel> _recipes = new();
        public ObservableCollection<RecipeViewModel> Recipes { get => _recipes; } // Updates locally when recipe is created/removed

        private GuidLinkedPart<RecipeViewModel>? _defaultRecipe;

        public GuidLinkedPart<RecipeViewModel>? LinkedDefaultRecipe { get => _defaultRecipe; set => SetProperty(ref _defaultRecipe, value); }

        private IconViewModel _icon;
        public IconViewModel Icon { get => _icon; private set => SetProperty(ref _icon, value); }

        // Info updating
        protected override Dictionary<string, Action<ResourceDto>> ConfigureUpdaters() => new()
        {
            { nameof(ResourceDto.Name), dto => Name = dto.Name },
            { nameof(ResourceDto.DefaultRecipeUid),
                dto => LinkedDefaultRecipe =
                dto.DefaultRecipeUid is Guid dRecipeUid
                ? _linkedPartsManager.CreateAndRegisterLinkedRecipeVM(dRecipeUid)
                : null},
            { nameof(ResourceDto.Icon), dto => Icon.UpdateFromDto(dto.Icon) },
        };

        private void OnResourceUpdated(ResourceUpdatedEvent ev)
        {
            if (Uid != ev.Resource.Uid) return;

            Update(ev.Resource, ev.ChangedProperties);
        }

        private void OnRecipeCreated(RecipeCreatedEvent ev)
        {
            if (Uid != ev.Recipe.ParentResourceUid) return;

            var recipeVM = _partsFactory.GetOrCreateRecipeVM(ev.Recipe);
            Recipes.Add(recipeVM);
        }

        private void OnRecipeDeleted(RecipeDeletedEvent ev)
        {
            if (Uid != ev.ParentResourceUid) return;

            var recipeVM = Recipes.FirstOrDefault(r => r.Uid == ev.RecipeUid);
            if (recipeVM != null)
            {
                Recipes.Remove(recipeVM);
                recipeVM.Dispose();
            }
        }

        private void OnRecipeMoved(RecipeMovedEvent ev)
        {
            if (Uid == ev.OldResourceUid)
            {
                var recipeVM = Recipes.FirstOrDefault(c => c.Uid == ev.RecipeUid);
                if (recipeVM != null)
                    Recipes.Remove(recipeVM);
            }
            else if (Uid == ev.NewResourceUid)
            {
                var recipeVM = _store.Recipes.GetValueOrDefault(ev.RecipeUid);
                if (recipeVM != null)
                {
                    Recipes.Add(recipeVM);
                    recipeVM.LinkedParentResource = _linkedPartsManager.CreateAndRegisterLinkedResourceVM(Uid);
                }
            }
        }

        /// <summary> Used when new DB is initialized and we need to connect created VM parts to each other </summary>
        internal void InitAddChild(RecipeViewModel recipe)
        {
            Recipes.Add(recipe);
        }

        public void Dispose()
        {
            UiItem.Dispose();

            _updatedSubscription.Dispose();
            _childAddSubscription.Dispose();
            _childRemoveSubscription.Dispose();
            _childMoveSubscription.Dispose();

            foreach (var recipe in  Recipes)
                recipe.Dispose();

            _store.RemoveResource(Uid);
        }

        // Commands
        [RelayCommand]
        public async Task SetDefaultRecipeUidAsync(Guid recipeUid)
        {
            await _commands.CreateAsyncEndExcecuteAsync<SetDefaultRecipeToResourceCommand>(Uid, recipeUid);
        }

        // For UI
        public ResourceItemUIState UiItem => _uiStateService.GetOrCreateItemUi(this);
        PartItemUIState IVMPart.UiItem => UiItem;
        public PartsGlobalNavigations GlobalNavigations { get; }
    }
}
