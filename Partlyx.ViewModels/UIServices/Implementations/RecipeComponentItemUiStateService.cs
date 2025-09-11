using Partlyx.ViewModels.PartsViewModels;
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
        private readonly Dictionary<Guid, RecipeComponentUIState> _states = new();

        public RecipeComponentUIState GetOrCreate(Guid id)
        {
            return _states.TryGetValue(id, out var state) ? state : (_states[id] = new RecipeComponentUIState());
        }
    }
}
