using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public class SelectedPartsObserveHelper : IDisposable
    {
        private readonly ICollection<object> _parts;

        private readonly ISelectedParts _selectedPartsContainer;

        public SelectedPartsObserveHelper(ISelectedParts selectedParts, INotifyCollectionChanged selectedCollection)
        {
            _parts = (ICollection<object>)selectedCollection;
            selectedCollection.CollectionChanged += OnSelectedItemsChanged;

            _selectedPartsContainer = selectedParts;
        }

        private void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems?.Count == 1 && (sender as ICollection<IVMPart>)?.Count == 1)
            {
                OnItemsCleared();
            }
            if (args.NewItems != null)
            {
                foreach (var itemToAdd in args.NewItems)
                {
                    OnItemSelected(itemToAdd);
                }
            }

            if (args.OldItems != null)
            {
                foreach (var itemToAdd in args.OldItems)
                {
                    OnItemUnselected(itemToAdd);
                }
            }
        }
        private void OnItemsCleared()
        {
            var selectedParts = _selectedPartsContainer;
            if (selectedParts == null) return;

            selectedParts.ClearSelection();
        }
        private void OnItemUnselected(object item)
        {
            var selectedParts = _selectedPartsContainer;
            if (selectedParts == null) return;

            if (item is ResourceViewModel resource)
            {
                selectedParts.RemoveResourceFromSelected(resource);
            }
            else if (item is RecipeViewModel recipe)
            {
                selectedParts.RemoveRecipeFromSelected(recipe);
            }
            else if (item is RecipeComponentViewModel component)
            {
                selectedParts.RemoveComponentFromSelected(component);
            }
            else if (item is RecipeComponentInputGroup inputHeader)
            {
                foreach (var input in inputHeader.ParentRecipe.Inputs)
                {
                    input.UiItem.Unselect();
                }
            }
            else if (item is RecipeComponentOutputGroup outputHeader)
            {
                foreach (var output in outputHeader.ParentRecipe.Outputs)
                {
                    output.UiItem.Unselect();
                }
            }
        }
        private void OnItemSelected(object item)
        {
            var selectedParts = _selectedPartsContainer;
            if (selectedParts == null) return;

            if (item is ResourceViewModel resource)
            {
                selectedParts.AddResourceToSelected(resource);
            }
            else if (item is RecipeViewModel recipe)
            {
                selectedParts.AddRecipeToSelected(recipe);
            }
            else if (item is RecipeComponentViewModel component)
            {
                selectedParts.AddComponentToSelected(component);
            }
            else if (item is RecipeComponentInputGroup inputHeader)
            {
                foreach (var input in inputHeader.ParentRecipe.Inputs)
                {
                    input.UiItem.Select();
                }
            }
            else if (item is RecipeComponentOutputGroup outputHeader)
            {
                foreach (var output in outputHeader.ParentRecipe.Outputs)
                {
                    output.UiItem.Select();
                }
            }
        }

        public void Dispose()
        {
            ((INotifyCollectionChanged)_parts).CollectionChanged -= OnSelectedItemsChanged;
        }
    }
}
