using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class RecipeListViewModel : ObservableObject, IDisposable
    {
        private readonly ICommandFactory _commandFactory;
        private readonly ICommandDispatcher _commandDispatcher;

        private readonly IDisposable _bulkLoadedSubscription;
        private readonly IDisposable _selectedParentsChangedSubscription;

        public IGlobalSelectedParts SelectedParts { get; }

        // Recipes in this collection are ALWAYS only a projection of the selected item collection.
        // Therefore, Set is allowed here and it is not recommended to change the contents of the selected collection by reference.
        private ObservableCollection<RecipeItemViewModel> _recipes;
        public ObservableCollection<RecipeItemViewModel> Recipes { get => _recipes; private set => SetProperty(ref _recipes, value); }

        public RecipeListViewModel(IEventBus bus, IVMPartsFactory vmpf, ICommandFactory cf, ICommandDispatcher cd, IGlobalSelectedParts sp)
        {
            _commandFactory = cf;
            _commandDispatcher = cd;
            SelectedParts = sp;

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

        [RelayCommand]
        private async Task CreateRecipeAsync()
        {
            var parent = SelectedParts.GetSingleResourceOrNull();
            if (parent == null)
                throw new InvalidOperationException("Create command shouldn't be called when created part's parent isn't selected or is multiselected");

            var command = _commandFactory.Create<CreateRecipeCommand>(parent.Uid);
            await _commandDispatcher.ExcecuteAsync(command);
        }

        [RelayCommand]
        private void StartRenamingSelected()
        {
            var recipeVM = SelectedParts.GetSingleRecipeOrNull();
            if (recipeVM == null) return;

            recipeVM.Ui.IsRenaming = true;
        }
    }
}