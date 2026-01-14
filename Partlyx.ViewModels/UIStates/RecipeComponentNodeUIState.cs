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

        public RecipeComponentNodeUIState(RecipeComponentViewModel vm, PartsServiceViewModel svm, VMComponentsGraphs graphs)
        {
            _services = svm;

            _componentVM = vm;
        }

        [RelayCommand]
        public async Task SetNextSelectedRecipe()
        {
            var resource = _componentVM.Resource;

            if (resource == null)
                return;

            var selectedRecipe = _componentVM.LinkedSelectedRecipe?.Value;

            if (selectedRecipe == null)
            {
                var firstRecipe = resource.ProducingRecipes.FirstOrDefault();
                
                if (firstRecipe == null)
                    return;

                await _services.ComponentService.SetSelectedRecipe(_componentVM, firstRecipe, true);
            }
            else 
            {
                var selectedRecipeIndex = resource.ProducingRecipes.IndexOf(selectedRecipe);
                if (selectedRecipeIndex + 1 == resource.ProducingRecipes.Count)
                {
                    var args = new PartSetValueInfo<RecipeComponentViewModel, RecipeViewModel?>(_componentVM, null);
                    await _services.ComponentService.SetSelectedRecipe(_componentVM, null, true);
                }
                else
                {
                    var nextRecipe = resource.ProducingRecipes[selectedRecipeIndex + 1];
                    var args = new PartSetValueInfo<RecipeComponentViewModel, RecipeViewModel?>(_componentVM, nextRecipe);
                    await _services.ComponentService.SetSelectedRecipe(_componentVM, nextRecipe, true);
                }
            }
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        [RelayCommand]
        public void FindResourceInTree()
        {
            var resource = _componentVM.LinkedResource?.Value;
            if (resource == null) return;

            resource.UiItem.FindInTree();
        }

        [RelayCommand]
        public void FindInTree()
            => _componentVM.UiItem.FindInTree();

        [RelayCommand]
        public void OpenBranch()
        {
            var targetRecipe = _componentVM.LinkedSelectedRecipe?.Value ?? _componentVM.LinkedResource?.Value?.LinkedDefaultRecipe?.Value;
            if (targetRecipe != null)
            {
                targetRecipe.LinkedParentResource?.Value?.UiItem.Expand();
                targetRecipe.UiItem.Select();
                targetRecipe.UiItem.FocusGlobal();
                targetRecipe.UiItem.Expand();
                return;
            }

            var fallbackTargetResource = _componentVM.LinkedResource?.Value;
            if (fallbackTargetResource != null)
            {
                fallbackTargetResource.UiItem.Select();
                fallbackTargetResource.UiItem.FocusGlobal();
                return;
            }
        }
    }
}
