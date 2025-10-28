using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class TreeViewPartSelectionBehavior : TreeViewItemSelectBehaviorBase
    {
        public static readonly DependencyProperty SelectedPartsContainerProperty =
            DependencyProperty.Register(
                nameof(SelectedPartsContainer),
                typeof(ISelectedParts),
                typeof(TreeViewPartSelectionBehavior),
                new PropertyMetadata(null, OnSelectedPartsPropertyChanged));

        public ISelectedParts? SelectedPartsContainer
        {
            get => (ISelectedParts?)GetValue(SelectedPartsContainerProperty);
            set => SetValue(SelectedPartsContainerProperty, value);
        }

        public static readonly DependencyProperty FocusedPartContainerProperty =
            DependencyProperty.Register(
                nameof(FocusedPartContainer),
                typeof(IFocusedPart),
                typeof(TreeViewPartSelectionBehavior),
                new PropertyMetadata(null, OnSelectedPartsPropertyChanged));

        public IFocusedPart? FocusedPartContainer
        {
            get => (IFocusedPart?)GetValue(FocusedPartContainerProperty);
            set => SetValue(FocusedPartContainerProperty, value);
        }

        public static readonly DependencyProperty AutoSelectAncestorPartsProperty =
            DependencyProperty.Register(
                nameof(AutoSetAncestorParts), 
                typeof(bool), 
                typeof(TreeViewPartSelectionBehavior),
                new PropertyMetadata(false, OnAutoSelectAncestorPartsPropertyChanged));

        public bool AutoSetAncestorParts
        {
            get => (bool)GetValue(AutoSelectAncestorPartsProperty);
            set => SetValue(AutoSelectAncestorPartsProperty, value);
        }

        public static readonly DependencyProperty AutoSelectDefaultResourceRecipeProperty =
            DependencyProperty.Register(
                nameof(AutoSelectDefaultResourceRecipeProperty),
                typeof(bool),
                typeof(TreeViewPartSelectionBehavior),
                new PropertyMetadata(true, OnAutoSelectDefaultResourceRecipePropertyChanged));

        public bool AutoSelectDefaultResourceRecipe
        {
            get => (bool)GetValue(AutoSelectDefaultResourceRecipeProperty);
            set => SetValue(AutoSelectDefaultResourceRecipeProperty, value);
        }

        public static readonly DependencyProperty FocusSelectedPartProperty =
            DependencyProperty.Register(
                nameof(FocusSelectedPart),
                typeof(bool),
                typeof(TreeViewPartSelectionBehavior),
                new PropertyMetadata(false, OnAutoSelectAncestorPartsPropertyChanged));

        public bool FocusSelectedPart
        {
            get => (bool)GetValue(FocusSelectedPartProperty);
            set => SetValue(FocusSelectedPartProperty, value);
        }

        private static void OnAutoSelectAncestorPartsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (TreeViewPartSelectionBehavior)d;
            UpdateSelectedPartOfInstance(behavior);
        }

        private static void OnSelectedPartsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (TreeViewPartSelectionBehavior)d;
            UpdateSelectedPartOfInstance(behavior);
        }

        private static void OnAutoSelectDefaultResourceRecipePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (TreeViewPartSelectionBehavior)d;
            UpdateSelectedPartOfInstance(behavior);
        }

        private static void UpdateSelectedPartOfInstance(TreeViewPartSelectionBehavior behaviorInstance)
        {
            var control = behaviorInstance.AssociatedObject;
            if (control == null) return;

            var selected = control.SelectedItem as IVMPart;
            behaviorInstance.SetSelectedPart(selected);
        }

        protected override void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            base.OnSelectedItemChanged(sender, e);

            if (e.NewValue is not IVMPart part) return;

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
