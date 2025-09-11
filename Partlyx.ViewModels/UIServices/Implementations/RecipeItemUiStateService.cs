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

        public RecipeItemUIState GetOrCreate(Guid uid)
        {
            var state = _states.GetValueOrDefault(uid);
            if (state == null)
            {
                state = (RecipeItemUIState)ActivatorUtilities.CreateInstance(_provider, typeof(RecipeItemUIState), uid);
                _states.Add(uid, state);
            }
            return state;
        }
    }
}
