using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.ViewModels.PartsViewModels;
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
    public class RecipeComponentItemUiStateService : IRecipeComponentItemUiStateService
    {
        private IServiceProvider _provider { get; }
        public RecipeComponentItemUiStateService(IServiceProvider provider)
        {
            _provider = provider;
        }
        private readonly Dictionary<Guid, RecipeComponentUIState> _itemStates = new();
        private readonly Dictionary<Guid, RecipeComponentNodeUIState> _nodeStates = new();

        public RecipeComponentUIState GetOrCreateItemUi(RecipeComponentViewModel vm)
        {
            var state = _itemStates.GetValueOrDefault(vm.Uid);
            if (state == null)
            {
                state = (RecipeComponentUIState)ActivatorUtilities.CreateInstance(_provider, typeof(RecipeComponentUIState), vm);
                _itemStates.Add(vm.Uid, state);
            }
            return state;
        }

        public RecipeComponentNodeUIState GetOrCreateNodeUi(RecipeComponentViewModel vm)
        {
            var state = _nodeStates.GetValueOrDefault(vm.Uid);
            if (state == null)
            {
                state = (RecipeComponentNodeUIState)ActivatorUtilities.CreateInstance(_provider, typeof(RecipeComponentNodeUIState), vm);
                _nodeStates.Add(vm.Uid, state);
            }
            return state;
        }
    }
}
