using GongSolutions.Wpf.DragDrop;
using Microsoft.EntityFrameworkCore.Metadata;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Implementations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace Partlyx.UI.WPF.DragAndDrop
{
    class PartsTreeDropHandler : IDropTarget
    {
        private readonly DefaultDropHandler _defaultHandler = new DefaultDropHandler();
        private readonly PartsServiceViewModel _partsService;

        public PartsTreeDropHandler(PartsServiceViewModel service)
        {
            _partsService = service;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            var targetCollection = dropInfo.TargetCollection;
            if (targetCollection == null) return;
            var target = dropInfo.TargetItem;

            // Handle resource drag over
            if (DropInfoHelpers.TryGetItemsOfType<ResourceViewModel>(dropInfo, out var items1))
            {
                // Resource moving
                if (targetCollection is ObservableCollection<ResourceViewModel>)
                {
                    _defaultHandler.DragOver(dropInfo);
                }
                // Create component from resources
                else if (targetCollection is ObservableCollection<RecipeComponentViewModel>)
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                    dropInfo.DropTargetHintAdorner = DropTargetAdorners.Highlight;
                    return;
                }
                return;
            }
            // Handle recipe drag over
            else if (DropInfoHelpers.TryGetItemsOfType<RecipeViewModel>(dropInfo, out var items2))
            {
                // Recipe moving
                if (targetCollection is ObservableCollection<RecipeViewModel>)
                {
                    _defaultHandler.DragOver(dropInfo);
                }
                return;
            }
            // Handle component drag over
            else if (DropInfoHelpers.TryGetItemsOfType<RecipeComponentViewModel>(dropInfo, out var items3))
            {
                // component moving
                if (targetCollection is ObservableCollection<RecipeComponentViewModel>)
                {
                    _defaultHandler.DragOver(dropInfo);
                }
                return;
            }
            else
            {
                dropInfo.Effects = DragDropEffects.None;
                dropInfo.DropTargetHintAdorner = DropTargetAdorners.Hint;
                dropInfo.DropTargetHintState = DropHintState.Error;
            }
        }

        public async void Drop(IDropInfo dropInfo)
        {
            var targetCollection = dropInfo.TargetCollection;
            if (targetCollection == null) return;
            var target = dropInfo.TargetItem;
            var sourceCollection = dropInfo.DragInfo.SourceCollection;
            bool dropInsideDragSource = targetCollection == sourceCollection;

            // Handle resource drop
            if (DropInfoHelpers.TryGetItemsOfType<ResourceViewModel>(dropInfo, out var items))
            {
                // Resource moving
                if (targetCollection is ICollection<ResourceViewModel>)
                {
                    _defaultHandler.Drop(dropInfo);
                }
                // Create component from resources
                else if (targetCollection is ObservableCollection<RecipeComponentViewModel> componentsCollection)
                {
                    if (!dropInsideDragSource)
                    {
                        RecipeViewModel parent;
                        if (target is RecipeViewModel)
                            parent = (RecipeViewModel)target;
                        else if (target is RecipeComponentViewModel component)
                            parent = component.LinkedParentRecipe!.Value!;
                        else return;

                        await _partsService.ComponentService.CreateComponentsFromAsync(parent, items);
                    }
                    else
                        _defaultHandler.Drop(dropInfo);
                }
                return;
            }
            // Handle recipe drop
            else if (DropInfoHelpers.TryGetItemsOfType<RecipeViewModel>(dropInfo, out var items2))
            {
                // Recipe moving
                if (targetCollection is ICollection<RecipeViewModel>)
                {
                    if (!dropInsideDragSource)
                    {
                        ResourceViewModel parent;
                        if (target is ResourceViewModel)
                            parent = (ResourceViewModel)target;
                        else if (target is RecipeViewModel recipe)
                            parent = recipe.LinkedParentResource!.Value!;
                        else return;

                        var moveInfo = new PartsTargetInteractionInfo<RecipeViewModel, ResourceViewModel>(items2, parent);
                        await _partsService.RecipeService.MoveRecipesAsync(moveInfo);
                    }
                    else
                        _defaultHandler.Drop(dropInfo);
                }
                return;
            }
            // Handle component drop
            else if (DropInfoHelpers.TryGetItemsOfType<RecipeComponentViewModel>(dropInfo, out var items3))
            {
                // component moving
                if (targetCollection is ICollection<RecipeComponentViewModel>)
                {
                    if (!dropInsideDragSource)
                    {
                        RecipeViewModel parent;
                        if (target is RecipeViewModel)
                            parent = (RecipeViewModel)target;
                        else if (target is RecipeComponentViewModel component)
                            parent = component.LinkedParentRecipe!.Value!;
                        else return;

                        var moveInfo = new PartsTargetInteractionInfo<RecipeComponentViewModel, RecipeViewModel>(items3, parent);
                        await _partsService.ComponentService.MoveComponentsAsync(moveInfo);
                    }
                    else
                        _defaultHandler.Drop(dropInfo);
                }
                return;
            }
        }
    }
}
