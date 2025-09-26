using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
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
        private readonly Dictionary<Guid, RecipeComponentUIState> _states = new();

        public RecipeComponentUIState GetOrCreate(RecipeComponentItemViewModel vm)
        {
            var state = _states.GetValueOrDefault(vm.Uid);
            if (state == null)
            {
                state = (RecipeComponentUIState)ActivatorUtilities.CreateInstance(_provider, typeof(RecipeComponentUIState), vm);
                _states.Add(vm.Uid, state);
            }
            return state;
        }
    }
}
