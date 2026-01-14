namespace Partlyx.ViewModels.Graph.PartsGraph
{
    public interface IPartsGraphInstanceManager
    {
        PartsGraphBuilderViewModel ParentBuilder { get; }
        void AddNode(GraphNodeViewModel node) => ParentBuilder.AddNode(node);
        void BuildGraph();

        void UpdateCosts();
    }
}
