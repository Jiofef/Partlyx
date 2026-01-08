using Partlyx.ViewModels.GraphicsViewModels;
using System.Collections.ObjectModel;
using System.Drawing;
using UJL.CSharp.Collections;

namespace Partlyx.ViewModels.Graph
{
    public interface IGraphTreeBuilderViewModel
    {
        ObservableCollection<EdgeViewModel> Edges { get; }
        ReadOnlyObservableCollection<GraphNodeViewModel> Nodes { get; }
        GraphNodeViewModel? RootNode { get; }
        Point RootNodeDefaultPosition { get; }
        ReadOnlyDictionary<Guid, GraphNodeViewModel> NodesDictionary { get; }
    }
}
