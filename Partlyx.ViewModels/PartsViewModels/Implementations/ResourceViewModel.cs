using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.GlobalNavigations;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using Partlyx.ViewModels.UIStates;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public partial class ResourceViewModel : UpdatableViewModel<ResourceDto>, IVMPart, IObservableFindableIconHolder, ITypedVMPartHolder<ResourceViewModel>
    {
        // Services
        private readonly IVMPartsStore _store;
        private readonly IVMPartsFactory _partsFactory;
        private readonly IResourceItemUiStateService _uiStateService;
        private readonly ICommandServices _commands;
        private readonly ILinkedPartsManager _linkedPartsManager;
        private readonly IconServiceViewModel _iconService;
        private readonly IEventBus _bus;
        public PartsServiceViewModel Services { get; }
        private ResourceViewModel() { }
        public ResourceViewModel(ResourceDto dto, PartsServiceViewModel service, PartsGlobalNavigations nav, IVMPartsStore store, IVMPartsFactory partsFactory,
            IResourceItemUiStateService uiStateS, ICommandServices cs, ILinkedPartsManager lpm, IconServiceViewModel iconService, IEventBus bus)
        {
            Uid = dto.Uid;

            // Services
            Services = service;
            GlobalNavigations = nav;
            _store = store;
            _partsFactory = partsFactory;
            _uiStateService = uiStateS;
            _commands = cs;
            _linkedPartsManager = lpm;
            _iconService = iconService;
            _bus = bus;

            // Info
            _name = dto.Name;
            if (dto.DefaultRecipeUid is Guid dRecipeUid)
                _defaultRecipe = _linkedPartsManager.CreateAndRegisterLinkedRecipeVM(dRecipeUid);

            RecipesDic = new(_recipesDic);

            foreach (var recipe in dto.Recipes)
            {
                var vm = _partsFactory.GetOrCreateRecipeVM(recipe);
                _recipes.Add(vm);
                _recipesDic.Add(vm.Uid, vm);
            }

            // For the most part, we don't really care when the icon will be loaded. Until then, the icon will be empty.
            _icon = new IconViewModel();
            _ = UpdateIconFromDto(dto.Icon);
        }
        private async Task UpdateIconFromDto(IconDto dto)
        {
            Icon?.Dispose();
            Icon = await _iconService.CreateFromDtoAsync(dto);
        }

        // Resource info
        public PartTypeEnumVM PartType { get => PartTypeEnumVM.Resource; }

        public Guid Uid { get; }

        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private ObservableCollection<RecipeViewModel> _recipes = new();
        public ObservableCollection<RecipeViewModel> Recipes { get => _recipes; } // Updates locally when recipe is created/removed

        private readonly Dictionary<Guid, RecipeViewModel> _recipesDic = new();
        public ReadOnlyDictionary<Guid, RecipeViewModel> RecipesDic { get; }
        public RecipeViewModel? GetChildOrNull(Guid uid) => _recipesDic.GetValueOrDefault(uid);

        private void AddRecipe(RecipeViewModel recipe)
        {
            if (_recipesDic.ContainsKey(recipe.Uid))
                return;

            Recipes.Add(recipe);
            _recipesDic.Add(recipe.Uid, recipe);
        }
        private void RemoveRecipe(RecipeViewModel recipe)
        {
            if (!_recipesDic.ContainsKey(recipe.Uid))
                return;

            Recipes.Remove(recipe);
            _recipesDic.Remove(recipe.Uid);
        }

        private GuidLinkedPart<RecipeViewModel>? _defaultRecipe;

        public GuidLinkedPart<RecipeViewModel>? LinkedDefaultRecipe { get => _defaultRecipe; set => SetProperty(ref _defaultRecipe, value); }

        private IconViewModel _icon;
        public IconViewModel Icon { get => _icon; private set => SetProperty(ref _icon, value); }

        // Info updating
        protected override Dictionary<string, Action<ResourceDto>> ConfigureUpdaters() => new()
        {
            { nameof(ResourceDto.Name), dto => 
            Name = dto.Name },
            { nameof(ResourceDto.DefaultRecipeUid),
                dto => LinkedDefaultRecipe =
                dto.DefaultRecipeUid is Guid dRecipeUid
                ? _linkedPartsManager.CreateAndRegisterLinkedRecipeVM(dRecipeUid)
                : null},
            { nameof(ResourceDto.Icon), dto => _ = UpdateIconFromDto(dto.Icon) },
        };

        public void HandleEvent(object @event)
        {
            if (@event is ResourceUpdatedEvent rue)
            {
                OnResourceUpdated(rue);
                return;
            }
            if (@event is RecipeCreatedEvent rce)
            {
                OnRecipeCreated(rce);
                return;
            }    
            if (@event is RecipeDeletingStartedEvent rde)
            {
                OnRecipeDeletingStarted(rde);
                return;
            }
            if (@event is RecipeMovedEvent rme)
            {
                OnRecipeMoved(rme);
                return;
            }
        }

        private void OnResourceUpdated(ResourceUpdatedEvent ev)
        {
            if (Uid != ev.Resource.Uid) return;

            Update(ev.Resource, ev.ChangedProperties);
        }

        private void OnRecipeCreated(RecipeCreatedEvent ev)
        {
            if (Uid != ev.Recipe.ParentResourceUid) return;

            var recipeVM = _partsFactory.GetOrCreateRecipeVM(ev.Recipe);
            AddRecipe(recipeVM);

            UiItem.IsExpanded = true;
        }

        private void OnRecipeDeletingStarted(RecipeDeletingStartedEvent ev)
        {
            if (Uid != ev.ParentResourceUid) return;

            var recipeVM = Recipes.FirstOrDefault(r => r.Uid == ev.RecipeUid);
            if (recipeVM != null)
            {
                RemoveRecipe(recipeVM);
                recipeVM.Dispose();
            }
        }

        private void OnRecipeMoved(RecipeMovedEvent ev)
        {
            if (Uid == ev.OldResourceUid)
            {
                var recipeVM = _recipesDic.GetValueOrDefault(ev.RecipeUid);
                if (recipeVM != null)
                    RemoveRecipe(recipeVM);
            }
            else if (Uid == ev.NewResourceUid)
            {
                var recipeVM = _store.Recipes.GetValueOrDefault(ev.RecipeUid);
                if (recipeVM != null)
                {
                    AddRecipe(recipeVM);
                    recipeVM.LinkedParentResource = _linkedPartsManager.CreateAndRegisterLinkedResourceVM(Uid);
                }
            }
        }

        /// <summary> Used when new DB is initialized and we need to connect created VM parts to each other </summary>
        internal void InitAddChild(RecipeViewModel recipe)
        {
            AddRecipe(recipe);
        }

        public void Dispose()
        {
            UiItem.Dispose();

            foreach (var recipe in  Recipes)
                recipe.Dispose();
        }
        public override void Update(ResourceDto dto, IReadOnlyList<string>? changedProperties = null)
        {
            base.Update(dto, changedProperties);

            var eventRecieversKeys = new HashSet<object>();
            if (changedProperties != null)
                foreach (var property in changedProperties)
                    eventRecieversKeys.Add(property);
            eventRecieversKeys.Add(dto.Uid);
            _bus.Publish(new ResourceUpdatedViewModelEvent(Uid, changedProperties, eventRecieversKeys));
        }
        // Commands
        [RelayCommand]
        public async Task SetDefaultRecipeUidAsync(Guid recipeUid)
        {
            await Task.Run(async () =>
            {
                await _commands.CreateAsyncEndExcecuteAsync<SetDefaultRecipeToResourceCommand>(Uid, recipeUid);
            });
        }

        // For UI
        public ResourceItemUIState UiItem => _uiStateService.GetOrCreateItemUi(this);
        PartItemUIState IVMPart.UiItem => UiItem;
        public PartsGlobalNavigations GlobalNavigations { get; }

        // Compatibility
        /// <summary> Self </summary>
        public ResourceViewModel? Part => this;
    }
}
