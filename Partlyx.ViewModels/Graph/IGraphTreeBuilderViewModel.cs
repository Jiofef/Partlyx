using Partlyx.ViewModels.GraphicsViewModels;
using System.Collections.ObjectModel;
using System.Drawing;
using UJL.CSharp.Collections;

namespace Partlyx.ViewModels.Graph
{
    public interface IGraphTreeBuilderViewModel
    {
        ObservableMultiCollection<FromToLineViewModel> Edges { get; }
        ObservableCollection<GraphTreeNodeViewModel> Nodes { get; }
        GraphTreeNodeViewModel? RootNode { get; }
        Point RootNodeDefaultPosition { get; }
    }
}
