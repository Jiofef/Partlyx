using Partlyx.ViewModels.Graph.PartsGraph;
using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.Graph
{
    public static class GraphNodesViewModelExtensions
    {
        public static ResourceGraphNodeViewModel ToNode(this ResourceViewModel resource, GraphNodeViewModel? mainRelative = null)
            => new ResourceGraphNodeViewModel(resource, mainRelative);
        public static RecipeGraphNodeViewModel ToNode(this RecipeViewModel recipe, GraphNodeViewModel? mainRelative = null)
            => new RecipeGraphNodeViewModel(recipe, mainRelative);
        public static ComponentGraphNodeViewModel ToNode(this RecipeComponentViewModel component, GraphNodeViewModel? mainRelative = null)
            => new ComponentGraphNodeViewModel(component, mainRelative);
    }
}
