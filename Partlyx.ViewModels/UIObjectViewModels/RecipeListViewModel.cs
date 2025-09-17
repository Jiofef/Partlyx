using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.PartsViewModels;
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
        private readonly IVMPartsFactory _partsFactory;
        private readonly ICommandFactory _commandFactory;
        private readonly ICommandDispatcher _commandDispatcher;

        private readonly IDisposable _childAddSubscription;
        private readonly IDisposable _childRemoveSubscription;
        private readonly IDisposable _bulkLoadedSubscription;

        public IGlobalSelectedParts SelectedParts { get; }

        public ObservableCollection<RecipeItemViewModel> Recipes { get; } = new();

        public bool IsSingleResourceSelected() => SelectedParts.GetSingleResourceOrNull() != null;

        public RecipeListViewModel(IEventBus bus, IVMPartsFactory vmpf, ICommandFactory cf, ICommandDispatcher cd, IGlobalSelectedParts sp)
        {
            _partsFactory = vmpf;
            _commandFactory = cf;
            _commandDispatcher = cd;
            SelectedParts = sp;

            _childAddSubscription = bus.Subscribe<RecipeCreatedEvent>(OnRecipeCreated, true);
            _childRemoveSubscription = bus.Subscribe<RecipeDeletedEvent>(OnRecipeDeleted, true);
            _bulkLoadedSubscription = bus.Subscribe<RecipesBulkLoadedEvent>(OnRecipeBulkLoaded, true);

            Recipes = new ObservableCollection<RecipeItemViewModel>();
        }

        private void AddFromDto(RecipeDto dto)
        {
            var recipeVM = _partsFactory.GetOrCreateRecipeVM(dto);
            Recipes.Add(recipeVM);
        }
        private void OnRecipeCreated(RecipeCreatedEvent ev)
        {
            AddFromDto(ev.Recipe);
        }

        private void OnRecipeDeleted(RecipeDeletedEvent ev)
        {
            var recipeVM = Recipes.FirstOrDefault(c => c.Uid == ev.RecipeUid);
            if (recipeVM != null)
            {
                Recipes.Remove(recipeVM);
                recipeVM.Dispose();
            }
        }

        private void OnRecipeBulkLoaded(RecipesBulkLoadedEvent ev)
        {
            Recipes.Clear();

            foreach (var dto in ev.Bulk)
                AddFromDto(dto);
        }

        public void Dispose()
        {
            _childAddSubscription.Dispose();
            _childRemoveSubscription.Dispose();
        }

        [RelayCommand]
        private async Task CreateRecipeAsync()
        {
            var parent = SelectedParts.GetSingleResourceOrNull();
            if (parent == null)
                throw new InvalidOperationException("Create command shouldn't be called when created part's parent isn't selected");

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