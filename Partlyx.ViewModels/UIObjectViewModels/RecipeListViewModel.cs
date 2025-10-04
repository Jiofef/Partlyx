using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class RecipeListViewModel : ObservableObject, IDisposable
    {
        private readonly IDisposable _bulkLoadedSubscription;
        private readonly IDisposable _selectedParentsChangedSubscription;

        public IGlobalSelectedParts SelectedParts { get; }
        public RecipeServiceViewModel Service { get; }

        // Recipes in this collection are ALWAYS only a projection of the selected item collection.
        // Therefore, Set is allowed here and it is not recommended to change the contents of the selected collection by reference.
        private ObservableCollection<RecipeItemViewModel> _recipes;
        public ObservableCollection<RecipeItemViewModel> Recipes { get => _recipes; private set => SetProperty(ref _recipes, value); }

        public RecipeListViewModel(IEventBus bus, IGlobalSelectedParts sp, RecipeServiceViewModel service)
        {
            SelectedParts = sp;
            Service = service;

            _bulkLoadedSubscription = 
            _selectedParentsChangedSubscription = bus.Subscribe<GlobalSelectedResourcesChangedEvent>(OnSelectedResourcesChanged, true);

            _recipes = new ObservableCollection<RecipeItemViewModel>();
        }

        public void UpdateList()
        {
            Recipes = new();
            var singleSelectedResource = SelectedParts.GetSingleResourceOrNull();
            if (singleSelectedResource == null) return;

            Recipes = singleSelectedResource.Recipes;
        }

        public void OnSelectedResourcesChanged(GlobalSelectedResourcesChangedEvent ev)
        {
            UpdateList();
        }

        public void Dispose()
        {
            _bulkLoadedSubscription.Dispose();
            _selectedParentsChangedSubscription.Dispose();
        }
    }
}