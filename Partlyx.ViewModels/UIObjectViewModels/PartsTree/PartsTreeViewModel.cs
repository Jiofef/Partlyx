using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using UJL.CSharp.Collections;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public record TreeSearchQueryEvent(string queryText, PartTypeEnumVM? searchablePartType = null);

    public partial class PartsTreeViewModel : PartlyxObservable
    {
        private readonly SelectedPartsObserveHelper _selectionHelper;
        // Sub-view models for tabs
        public PartsTreeResourcesViewModel ResourcesTab { get; }
        public PartsTreeRecipesViewModel RecipesTab { get; }

        public enum TabEnum { Resources, Recipes }
        private TabEnum _selectedTab;
        public TabEnum SelectedTab { get => _selectedTab; set => SetProperty(ref _selectedTab, value); }

        //
        public IGlobalSelectedParts SelectedParts { get; }
        public IGlobalFocusedElementContainer FocusedPart { get; }
        public IResourceSearchService Search { get; }
        public PartsServiceViewModel Service { get; }
        public PartsTreeViewContextMenuCommands ContextMenuCommands { get; }

        private IGlobalResourcesVMContainer _resourcesContainer { get; }
        public ObservableCollection<ResourceViewModel> Resources => _resourcesContainer.Resources;
        private IGlobalRecipesVMContainer _recipesContainer { get; }
        public ObservableCollection<RecipeViewModel> Recipes => _recipesContainer.Recipes;
        public ReadOnlyObservableCollection<object> SelectedPartsCollection => _selectedPartsCollection; public PartsSelectionState SelectedPartsDetails { get; }
        private readonly ReadOnlyObservableCollection<object> _selectedPartsCollection;

        private bool _allowHotkeys = true;
        public bool AllowHotkeys { get => _allowHotkeys; private set => SetProperty(ref _allowHotkeys, value); }
        private void UpdateAllowHotkeys()
        {
            AllowHotkeys = _hotkeysBlockElementsAmount == 0;
        }
        private int _hotkeysBlockElementsAmount;

        public int HotkeysBlockElementsAmount 
        {
            get => _hotkeysBlockElementsAmount;
            set
            {
                if (SetProperty(ref _hotkeysBlockElementsAmount, value))
                    UpdateAllowHotkeys();
            }
        }

        public PartsTreeViewModel(IGlobalResourcesVMContainer grvmc, IGlobalRecipesVMContainer grc, IGlobalSelectedParts sp, IGlobalFocusedElementContainer fp, IEventBus bus,
                IResourceSearchService rss, PartsServiceViewModel service, PartsTreeResourcesViewModel ptr, PartsTreeRecipesViewModel ptrc)
        {
            _resourcesContainer = grvmc;
            _recipesContainer = grc;

            ResourcesTab = ptr;
            ResourcesTab.ParentTreeService = this;
            RecipesTab = ptrc;
            RecipesTab.ParentTreeService = this;

            var streamA = ResourcesTab.LocalSelectedPartsCollection.ToObservableChangeSet();
            var streamB = RecipesTab.LocalSelectedPartsCollection.ToObservableChangeSet();
            var allTheSelectedSubscription = streamA.Or(streamB)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _selectedPartsCollection)
                .Subscribe();
            Disposables.Add(allTheSelectedSubscription);

            SelectedParts = sp;
            FocusedPart = fp;
            Search = rss;
            Service = service;

            _selectionHelper = new SelectedPartsObserveHelper(SelectedParts, _selectedPartsCollection);
            Disposables.Add(_selectionHelper);

            SelectedPartsDetails = new PartsSelectionState(SelectedPartsCollection);
            ContextMenuCommands = new PartsTreeViewContextMenuCommands(this);
        }

        public void UpdateList()
        {
            ResourcesTab.UpdateList();
            RecipesTab.UpdateList();
        }


        [RelayCommand(CanExecute = nameof(AllowHotkeys))]
        public void ActivateHotkey(ICommand hotkeyCommand)
        {
            hotkeyCommand.Execute(null);
        }

        [RelayCommand]
        public async Task CreateChildrenForAllSelectedAsync()
        {
            // If any recipe selected, we want to know what components user wants to create for them
            ResourceViewModel[]? resourcesForComponentsCreate = null;
            if (SelectedPartsCollection.Any(p => p is RecipeViewModel))
            {
                var selected = await Service.ComponentService.ShowComponentCreateMenuAsync();
                if (selected != null)
                    resourcesForComponentsCreate = selected.Resources.ToArray();
            }

            foreach (var part in SelectedPartsCollection)
            {
                if (part is ResourceViewModel resource)
                {
                    await Service.RecipeService.CreateRecipeAsync(resource);
                }
                else if (part is RecipeViewModel recipe && resourcesForComponentsCreate != null)
                {
                    await Service.ComponentService.CreateComponentsFromAsync(recipe, resourcesForComponentsCreate);
                }
            }
        }
    }
}
