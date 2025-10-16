using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Implementations;
using ReactiveUI;
using System.Reactive.Linq;
namespace Partlyx.ViewModels.UIStates
{
    public partial class RecipeComponentNodeUIState : ObservableObject, IDisposable
    {
        private readonly PartsServiceViewModel _services;

        private readonly RecipeComponentViewModel _componentVM;

        private readonly List<IDisposable> _subscriptions = new();

        public RecipeComponentNodeUIState(RecipeComponentViewModel vm, PartsServiceViewModel svm)
        {
            _services = svm;

            _componentVM = vm;

            var columnTextUpdateSubscription = _componentVM
                .WhenAnyValue(c => c.LinkedResource!.Value!.Name)
                .Subscribe(n => UpdateColumnText());
            _subscriptions.Add(columnTextUpdateSubscription);

            var selectedRecipeUpdateSubscription = _componentVM
                .WhenAnyValue(c => c.LinkedSelectedRecipe!.Value!.Name)
                .Subscribe(n => UpdateBottomColumnText());

            UpdateColumnText();
            UpdateBottomColumnText();
        }

        private string _columnText = "";

        public string ColumnText { get => _columnText; private set => SetProperty(ref _columnText, value); }

        private string _bottomColumnText = "";
        public string BottomColumnText { get => _bottomColumnText; set => SetProperty(ref _bottomColumnText, value); }

        private void UpdateColumnText()
        {
            string? name = _componentVM.LinkedResource?.Value?.Name;

            if (name == null)
                ColumnText = "Null";
            else
            {
                string newText = name;

                // When different component display modes appear, the code here should be updated
                newText += $" x{_componentVM.Quantity}";
                ColumnText = newText;
            }
        }

        private void UpdateBottomColumnText()
        {
            string? selectedRecipeName = _componentVM.LinkedSelectedRecipe?.Value?.Name;

            if (selectedRecipeName == null)
            {
                BottomColumnText = "";
                return;
            }
            else
            {
                BottomColumnText = selectedRecipeName;
            }
        }

        [RelayCommand]
        public async Task SetNextSelectedRecipe()
        {
            if (_componentVM.LinkedResource?.Value?.Recipes == null || _componentVM.LinkedResource.Value.Recipes.Count > 0)
                return;

            ResourceViewModel resource = _componentVM.LinkedResource!.Value!;
            var selectedRecipe = _componentVM.LinkedSelectedRecipe?.Value;

            if (selectedRecipe == null)
            {
                var firstRecipe = resource.Recipes.First();
                var args = new PartSetValueInfo<RecipeComponentViewModel, RecipeViewModel?>(_componentVM, firstRecipe);
                await _services.ComponentService.SetSelectedRecipe(args);
            }
            else 
            {
                var selectedRecipeIndex = resource.Recipes.IndexOf(selectedRecipe);
                if (selectedRecipeIndex + 1 == resource.Recipes.Count)
                {
                    var args = new PartSetValueInfo<RecipeComponentViewModel, RecipeViewModel?>(_componentVM, null);
                    await _services.ComponentService.SetSelectedRecipe(args);
                }
                else
                {
                    var nextRecipe = resource.Recipes[selectedRecipeIndex + 1];
                    var args = new PartSetValueInfo<RecipeComponentViewModel, RecipeViewModel?>(_componentVM, nextRecipe);
                    await _services.ComponentService.SetSelectedRecipe(args);
                }
            }
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}
