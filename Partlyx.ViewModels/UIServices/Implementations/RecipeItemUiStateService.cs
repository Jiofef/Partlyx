using Microsoft.Extensions.DependencyInjection;
using Partlyx.ViewModels.Graph.PartsGraph;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using Partlyx.ViewModels.UIStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public class RecipeItemUiStateService : IRecipeItemUiStateService
    {
        private IServiceProvider _provider { get; }
        public RecipeItemUiStateService(IServiceProvider provider)
        {
            _provider = provider;
        }
        private readonly Dictionary<Guid, RecipeItemUIState> _itemStates = new();
        private readonly Dictionary<Guid, RecipeNodeUIState> _nodeStates = new();

        public RecipeItemUIState GetOrCreateItemUi(RecipeViewModel vm)
        {
            var state = _itemStates.GetValueOrDefault(vm.Uid);
            if (state == null)
            {
                state = (RecipeItemUIState)ActivatorUtilities.CreateInstance(_provider, typeof(RecipeItemUIState), vm);
                _itemStates.Add(vm.Uid, state);
            }
            return state;
        }
        public RecipeNodeUIState GetOrCreateNodeUi(RecipeViewModel vm)
        {
            var state = _nodeStates.GetValueOrDefault(vm.Uid);
            if (state == null)
            {
                state = (RecipeNodeUIState)ActivatorUtilities.CreateInstance(_provider, typeof(RecipeNodeUIState), vm);
                _nodeStates.Add(vm.Uid, state);
            }
            return state;
        }
    }

    public class ComponentPathUiStateService : IComponentPathUiStateService
    {
        private IServiceProvider _provider { get; }
        public ComponentPathUiStateService(IServiceProvider provider)
        {
            _provider = provider;
        }
        private readonly Dictionary<Guid, RecipeComponentPathItemUIState> _itemStates = new();

        public RecipeComponentPathItemUIState GetOrCreateItemUi(RecipeComponentPathItem vm)
        {
            var state = _itemStates.GetValueOrDefault(vm.Uid);
            if (state == null)
            {
                state = (RecipeComponentPathItemUIState)ActivatorUtilities.CreateInstance(_provider, typeof(RecipeComponentPathItemUIState), vm);
                _itemStates.Add(vm.Uid, state);
            }
            return state;
        }
    }
}
