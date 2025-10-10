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

namespace Partlyx.UI.WPF.Behaviors
{
    public class TreeViewPartSelectionBehavior : TreeViewItemSelectBehaviorBase
    {
        public static readonly DependencyProperty SelectedPartsProperty =
            DependencyProperty.Register(
                nameof(SelectedParts),
                typeof(ISelectedParts),
                typeof(TreeViewPartSelectionBehavior),
                new PropertyMetadata(null, OnSelectedPartsPropertyChanged));

        public ISelectedParts? SelectedParts
        {
            get => (ISelectedParts?)GetValue(SelectedPartsProperty);
            set => SetValue(SelectedPartsProperty, value);
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
            var selectedParts = SelectedParts;
            if (selectedParts == null) return;

            if (part is ResourceItemViewModel resource)
            {
                selectedParts.ClearSelection();
                selectedParts.SelectSingleResource(resource);

                if (AutoSelectDefaultResourceRecipe && resource.LinkedDefaultRecipe?.Value != null)
                    selectedParts.SelectSingleRecipe(resource.LinkedDefaultRecipe.Value);
            }
            else if (part is RecipeItemViewModel recipe)
            {
                selectedParts.ClearSelection();
                selectedParts.SelectSingleRecipe(recipe);

                if (AutoSetAncestorParts)
                    selectedParts.SelectSingleRecipeAncestor(recipe);
            }
            else if (part is RecipeComponentItemViewModel component)
            {
                selectedParts.ClearSelection();
                selectedParts.SelectSingleComponent(component);

                if (AutoSetAncestorParts)
                    selectedParts.SelectSingleComponentAncestors(component);
            }
        }
    }
}
