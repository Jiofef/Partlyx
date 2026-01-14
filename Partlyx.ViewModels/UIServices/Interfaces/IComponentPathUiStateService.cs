using Partlyx.ViewModels.Graph.PartsGraph;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIStates;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IComponentPathUiStateService
    {
        RecipeComponentPathItemUIState GetOrCreateItemUi(RecipeComponentPathItem vm);
    }
}