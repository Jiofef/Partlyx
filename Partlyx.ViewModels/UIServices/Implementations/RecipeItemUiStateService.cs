using Microsoft.Extensions.DependencyInjection;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.UIServices.Interfaces;
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
        private readonly Dictionary<Guid, RecipeItemUIState> _states = new();

        public RecipeItemUIState GetOrCreate(RecipeItemViewModel vm)
        {
            var state = _states.GetValueOrDefault(vm.Uid);
            if (state == null)
            {
                state = (RecipeItemUIState)ActivatorUtilities.CreateInstance(_provider, typeof(RecipeItemUIState), vm);
                _states.Add(vm.Uid, state);
            }
            return state;
        }
    }
}
