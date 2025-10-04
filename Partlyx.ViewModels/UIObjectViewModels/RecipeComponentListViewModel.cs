using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
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
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
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
        private readonly IDisposable _selectedParentsChangedSubscription;

        public IGlobalSelectedParts SelectedParts { get; }
        public RecipeComponentServiceViewModel Service { get; }

        // Components in this collection are ALWAYS only a projection of the selected item collection.
        // Therefore, Set is allowed here and it is not recommended to change the contents of the selected collection by reference.
        private ObservableCollection<RecipeComponentItemViewModel> _components;
        public ObservableCollection<RecipeComponentItemViewModel> Components { get => _components; private set => SetProperty(ref _components, value); }

        public RecipeComponentListViewModel(IEventBus bus, IGlobalSelectedParts sp, RecipeComponentServiceViewModel service)
        {
            SelectedParts = sp;
            Service = service;

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
    }
}
