using Avalonia;
using Avalonia.Controls;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class TreeViewPartSelectionBehavior : TreeViewItemSelectBehaviorBase
    {
        public static readonly StyledProperty<ISelectedParts?> SelectedPartsContainerProperty =
            AvaloniaProperty.Register<TreeViewPartSelectionBehavior, ISelectedParts?>(
                nameof(SelectedPartsContainer));

        public ISelectedParts? SelectedPartsContainer
        {
            get => GetValue(SelectedPartsContainerProperty);
            set => SetValue(SelectedPartsContainerProperty!, value);
        }

        public static readonly StyledProperty<IFocusedPart?> FocusedPartContainerProperty =
            AvaloniaProperty.Register<TreeViewPartSelectionBehavior, IFocusedPart?>(
                nameof(FocusedPartContainer));

        public IFocusedPart? FocusedPartContainer
        {
            get => GetValue(FocusedPartContainerProperty);
            set => SetValue(FocusedPartContainerProperty!, value);
        }

        public static readonly StyledProperty<bool> AutoSelectAncestorPartsProperty =
            AvaloniaProperty.Register<TreeViewPartSelectionBehavior, bool>(
                nameof(AutoSetAncestorParts),
                defaultValue: false);

        public bool AutoSetAncestorParts
        {
            get => GetValue(AutoSelectAncestorPartsProperty);
            set => SetValue(AutoSelectAncestorPartsProperty, value);
        }

        public static readonly StyledProperty<bool> AutoSelectDefaultResourceRecipeProperty =
            AvaloniaProperty.Register< TreeViewPartSelectionBehavior, bool>(
                nameof(AutoSelectDefaultResourceRecipeProperty),
                defaultValue: true);

        public bool AutoSelectDefaultResourceRecipe
        {
            get => GetValue(AutoSelectDefaultResourceRecipeProperty);
            set => SetValue(AutoSelectDefaultResourceRecipeProperty, value);
        }

        public static readonly StyledProperty<bool> FocusSelectedPartProperty =
            AvaloniaProperty.Register<TreeViewPartSelectionBehavior, bool>(
                nameof(FocusSelectedPart),
                defaultValue: false);

        public bool FocusSelectedPart
        {
            get => GetValue(FocusSelectedPartProperty);
            set => SetValue(FocusSelectedPartProperty, value);
        }

        static TreeViewPartSelectionBehavior()
        {
            SelectedPartsContainerProperty.Changed.AddClassHandler<TreeViewPartSelectionBehavior>((x, e) => x.UpdateSelectedPartOfInstance());
            FocusedPartContainerProperty.Changed.AddClassHandler<TreeViewPartSelectionBehavior>((x, e) => x.UpdateSelectedPartOfInstance());
            AutoSelectAncestorPartsProperty.Changed.AddClassHandler<TreeViewPartSelectionBehavior>((x, e) => x.UpdateSelectedPartOfInstance());
            AutoSelectDefaultResourceRecipeProperty.Changed.AddClassHandler<TreeViewPartSelectionBehavior>((x, e) => x.UpdateSelectedPartOfInstance());
            FocusSelectedPartProperty.Changed.AddClassHandler<TreeViewPartSelectionBehavior>((x, e) => x.UpdateSelectedPartOfInstance());
        }

        private void UpdateSelectedPartOfInstance()
        {
            var control = AssociatedObject;
            if (control == null) return;

            var selected = control.SelectedItem as IVMPart;
            SetSelectedPart(selected);
        }

        protected override void OnSelectedItemChanged(object? sender, SelectionChangedEventArgs args)
        {
            base.OnSelectedItemChanged(sender, args);

            if (args.AddedItems.Count == 0 || args.AddedItems[0] is not IVMPart part) return;

            SetSelectedPart(part);
        }

        private void SetSelectedPart(IVMPart? part)
        {
            var selectedParts = SelectedPartsContainer;
            if (selectedParts != null)
            {
                if (part is ResourceViewModel resource && !selectedParts.Resources.Contains(resource))
                {
                    selectedParts.SelectSingleResource(resource);

                    if (AutoSelectDefaultResourceRecipe && resource.LinkedDefaultRecipe?.Value != null)
                        selectedParts.SelectSingleRecipe(resource.LinkedDefaultRecipe.Value);
                    else
                        selectedParts.ClearSelectedRecipes();
                }
                else if (part is RecipeViewModel recipe && !selectedParts.Recipes.Contains(recipe))
                {
                    selectedParts.SelectSingleRecipe(recipe);

                    if (AutoSetAncestorParts)
                        selectedParts.SelectSingleRecipeAncestor(recipe);
                    else
                        selectedParts.ClearSelectedResources();
                    selectedParts.ClearSelectedComponents();
                }
                else if (part is RecipeComponentViewModel component && !selectedParts.Components.Contains(component))
                {
                    selectedParts.SelectSingleComponent(component);

                    if (AutoSetAncestorParts)
                        selectedParts.SelectSingleComponentAncestors(component);
                    else
                    {
                        selectedParts.ClearSelectedRecipes();
                        selectedParts.ClearSelectedResources();
                    }
                }
            }

            if (FocusSelectedPart)
            {
                var focusedPart = FocusedPartContainer;
                focusedPart?.FocusPart(part);
            }
        }
    }
}
