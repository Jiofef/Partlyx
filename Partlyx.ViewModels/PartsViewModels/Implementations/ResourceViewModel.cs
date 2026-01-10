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
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.Core.Partlyx;
using System.Runtime.CompilerServices;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public partial class ResourceViewModel : UpdatableViewModel<ResourceDto>, IVMPart, IObservableFindableIconHolder, ITypedVMPartHolder<ResourceViewModel>
    {
        // Services
        private readonly IResourceItemUiStateService _uiStateService;
        private readonly ICommandServices _commands;
        private readonly ILinkedPartsManager _linkedPartsManager;
        private readonly IconServiceViewModel _iconService;
        private readonly IEventBus _bus;
        public PartsServiceViewModel Services { get; }
        private ResourceViewModel() { }
        public ResourceViewModel(ResourceDto dto, PartsServiceViewModel service, PartsGlobalNavigations nav,
            IResourceItemUiStateService uiStateS, ICommandServices cs, ILinkedPartsManager lpm, IconServiceViewModel iconService, IEventBus bus)
        {
            Uid = dto.Uid;

            // Services
            Services = service;
            GlobalNavigations = nav;
            _uiStateService = uiStateS;
            _commands = cs;
            _linkedPartsManager = lpm;
            _iconService = iconService;
            _bus = bus;

            // Info
            _name = dto.Name;
            if (dto.DefaultRecipeUid is Guid dRecipeUid)
                _defaultRecipe = _linkedPartsManager.CreateAndRegisterLinkedRecipeVM(dRecipeUid);

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

        private GuidLinkedPart<RecipeViewModel>? _defaultRecipe;

        public GuidLinkedPart<RecipeViewModel>? LinkedDefaultRecipe { get => _defaultRecipe; set => SetProperty(ref _defaultRecipe, value); }

        private IconViewModel _icon;
        public IconViewModel Icon { get => _icon; private set => SetProperty(ref _icon, value); }

        // Recipe relationships
        private readonly HashSet<RecipeViewModel> _producingRecipesSet = new();
        public IReadOnlySet<RecipeViewModel> ProducingRecipesSet => _producingRecipesSet;
        public ObservableCollection<RecipeViewModel> ProducingRecipes { get; } = new();
        private readonly HashSet<RecipeViewModel> _receiverRecipesSet = new();
        public IReadOnlySet<RecipeViewModel> ReceiverRecipesSet => _receiverRecipesSet;
        public ObservableCollection<RecipeViewModel> ReceiverRecipes { get; } = new();

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
            if (@event is RecipeResourceLinkChangedEvent rrlce)
            {
                OnRecipeResourceLinkChanged(rrlce);
                return;
            }
        }

        private void OnResourceUpdated(ResourceUpdatedEvent ev)
        {
            if (Uid != ev.Resource.Uid) return;

            Update(ev.Resource, ev.ChangedProperties);
        }

        private void OnRecipeResourceLinkChanged(RecipeResourceLinkChangedEvent ev)
        {
            if (ev.ResourceUid != Uid) return;

            var recipe = ev.Recipe;

            void EnsureRemoveProducingRecipe()
            {
                if (_producingRecipesSet.Remove(recipe))
                    ProducingRecipes.Remove(recipe);
            }
            void EnsureAddProducingRecipe()
            {
                if (_producingRecipesSet.Add(recipe))
                    ProducingRecipes.Add(recipe);
            }
            void EnsureRemoveReceiverRecipe()
            {
                if (_receiverRecipesSet.Remove(recipe))
                    ReceiverRecipes.Remove(recipe);
            }
            void EnsureAddReceiverRecipe()
            {
                if (_receiverRecipesSet.Add(recipe))
                    ReceiverRecipes.Add(recipe);
            }

            switch (ev.LinkType)
            {
                case RecipeResourceLinkTypeEnum.None:
                    EnsureRemoveProducingRecipe();
                    EnsureRemoveReceiverRecipe();
                break;
                case RecipeResourceLinkTypeEnum.Receiving:
                    EnsureRemoveProducingRecipe();
                    EnsureAddReceiverRecipe();
                break;
                case RecipeResourceLinkTypeEnum.Producing:
                    EnsureAddProducingRecipe();
                    EnsureRemoveReceiverRecipe();
                break;
                    case RecipeResourceLinkTypeEnum.Both:
                    EnsureAddProducingRecipe();
                    EnsureAddReceiverRecipe();
                break;
            }
        }

        public void Dispose()
        {
            UiItem.Dispose();
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
                await _commands.CreateAndExcecuteAsync<SetDefaultRecipeToResourceCommand>(Uid, recipeUid);
            });
        }

        // For UI
        public ResourceItemUIState UiItem => _uiStateService.GetOrCreateItemUi(this);
        FocusableItemUIState IFocusable.UiItem => UiItem;
        public PartsGlobalNavigations GlobalNavigations { get; }

        // Compatibility
        /// <summary> Self </summary>
        public ResourceViewModel Part => this;
    }
}
