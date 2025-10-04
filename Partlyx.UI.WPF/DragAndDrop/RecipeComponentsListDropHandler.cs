using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Partlyx.UI.WPF.DragAndDrop
{
    public class RecipeComponentsListDropHandler : IDropTarget
    {
        private readonly DefaultDropHandler _defaultHandler = new DefaultDropHandler();
        private readonly PartsServiceViewModel _partsService;

        public RecipeComponentsListDropHandler(PartsServiceViewModel service) 
        {
            _partsService = service;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            _defaultHandler.DragOver(dropInfo);

            if (DropInfoHelpers.TryGetItemsOfType<ResourceItemViewModel>(dropInfo, out var items))
            {
                dropInfo.Effects = DragDropEffects.Copy;
                dropInfo.DropTargetHintAdorner = DropTargetAdorners.Highlight;
            }
        }

        public async void Drop(IDropInfo dropInfo)
        {
            if (DropInfoHelpers.TryGetItemsOfType<ResourceItemViewModel>(dropInfo, out var items))
            {
                // Create component from resources
                await _partsService.ComponentService.CreateComponentsFromAsync(items);
            }
            else
                _defaultHandler.Drop(dropInfo);
        }
    }
}
