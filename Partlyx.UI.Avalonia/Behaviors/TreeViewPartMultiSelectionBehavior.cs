using Avalonia;
using Avalonia.Controls;
using DynamicData;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Linq;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class TreeViewPartMultiSelectionBehavior : TreeViewItemSelectBehaviorBase
    {
        public static readonly StyledProperty<ISelectedParts?> SelectedPartsContainerProperty =
            AvaloniaProperty.Register<TreeViewPartMultiSelectionBehavior, ISelectedParts?>(
                nameof(SelectedPartsContainer));

        public ISelectedParts? SelectedPartsContainer
        {
            get => GetValue(SelectedPartsContainerProperty);
            set => SetValue(SelectedPartsContainerProperty!, value);
        }

        static TreeViewPartMultiSelectionBehavior()
        {
            SelectedPartsContainerProperty.Changed.AddClassHandler<TreeViewPartMultiSelectionBehavior>((x, e) => x.UpdateSelectedPartsOfInstance());
        }

        private void UpdateSelectedPartsOfInstance()
        {
            var selectedParts = SelectedPartsContainer;
            if (selectedParts == null) return;

            var control = AssociatedObject;
            if (control == null) return;

            // Deleting objects that are not selected
            var items = control.SelectedItems;
            foreach (var resource in selectedParts.Resources.ToList())
            {
                if (!items.Contains(resource))
                    selectedParts.RemoveResourceFromSelected(resource);
            }
            foreach (var recipe in selectedParts.Recipes.ToList())
            {
                if (!items.Contains(recipe))
                    selectedParts.RemoveRecipeFromSelected(recipe);
            }
            foreach (var component in selectedParts.Components.ToList())
            {
                if (!items.Contains(component))
                    selectedParts.RemoveComponentFromSelected(component);
            }

            // Adding new objects that are selected (if the object is added to selectedParts already, AddPartToSelected won't add it)
            foreach (var item in items)
            {
                if (item is IVMPart part)
                {
                    selectedParts.AddPartToSelected(part);
                }
            }
        }

        protected override void OnSelectedItemsChanged(object? sender, SelectionChangedEventArgs args)
        {
            base.OnSelectedItemsChanged(sender, args);

            if (args.AddedItems.Count == 1 && AssociatedObject?.SelectedItems.Count == 1)
            {
                OnItemsCleared();
            }

            foreach (var itemToAdd in args.AddedItems)
            {
                if (itemToAdd is IVMPart part)
                    OnItemSelected(part);
            }

            foreach (var itemToAdd in args.RemovedItems)
            {
                if (itemToAdd is IVMPart part)
                    OnItemUnselected(part);
            }
        }
        private void OnItemsCleared()
        {
            var selectedParts = SelectedPartsContainer;
            if (selectedParts == null) return;

            selectedParts.ClearSelection();
        }
        private void OnItemUnselected(IVMPart part)
        {
            var selectedParts = SelectedPartsContainer;
            if (selectedParts == null) return;

            if (part is ResourceViewModel resource)
            {
                selectedParts.RemoveResourceFromSelected(resource);
            }
            else if (part is RecipeViewModel recipe)
            {
                selectedParts.RemoveRecipeFromSelected(recipe);
            }
            else if (part is RecipeComponentViewModel component)
            {
                selectedParts.RemoveComponentFromSelected(component);
            }
        }
        private void OnItemSelected(IVMPart part)
        {
            var selectedParts = SelectedPartsContainer;
            if (selectedParts == null) return;

            if (part is ResourceViewModel resource)
            {
                selectedParts.AddResourceToSelected(resource);
            }
            else if (part is RecipeViewModel recipe)
            {
                selectedParts.AddRecipeToSelected(recipe);
            }
            else if (part is RecipeComponentViewModel component)
            {
                selectedParts.AddComponentToSelected(component);
            }
        }
    }
}
