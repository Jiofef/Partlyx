using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Core;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
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
    public partial class RecipeComponentListViewModel : ObservableObject, IDisposable
    {
        private readonly ICommandFactory _commandFactory;
        private readonly ICommandDispatcher _commandDispatcher;

        private readonly IDisposable _selectedParentsChangedSubscription;

        public IGlobalSelectedParts SelectedParts { get; }

        private ObservableCollection<RecipeComponentItemViewModel> _components;
        public ObservableCollection<RecipeComponentItemViewModel> Components { get => _components; private set => SetProperty(ref _components, value); }

        public RecipeComponentListViewModel(IEventBus bus, IVMPartsFactory vmpf, ICommandFactory cf, ICommandDispatcher cd, IGlobalSelectedParts sp)
        {
            _commandFactory = cf;
            _commandDispatcher = cd;
            SelectedParts = sp;

            _selectedParentsChangedSubscription = bus.Subscribe<GlobalSelectedRecipesChangedEvent>(OnSelectedRecipesChanged, true);

            _components = new ObservableCollection<RecipeComponentItemViewModel>();
        }

        public void UpdateList()
        {
            Components = new();
            var singleSelectedRecipe = SelectedParts.GetSingleRecipeOrNull();
            if (singleSelectedRecipe == null) return;

            Components = singleSelectedRecipe.Components;
        }

        public void OnSelectedRecipesChanged(GlobalSelectedRecipesChangedEvent ev)
        {
            UpdateList();
        }

        public void Dispose()
        {
            _selectedParentsChangedSubscription.Dispose();
        }

        [RelayCommand]
        private async Task CreateComponentAsync()
        {
            var parent = SelectedParts.GetSingleRecipeOrNull();
            if (parent == null)
                throw new InvalidOperationException("Create command shouldn't be called when created part's parent isn't selected or is multiselected");

            var command = _commandFactory.Create<CreateRecipeComponentCommand>(parent.ParentResource!, parent.Uid);
            await _commandDispatcher.ExcecuteAsync(command);
        }
    }
}
