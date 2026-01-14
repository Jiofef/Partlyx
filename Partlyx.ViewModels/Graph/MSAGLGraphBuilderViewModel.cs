using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Layout.Layered;
using Partlyx.UI.Avalonia.Helpers;
using Partlyx.ViewModels.GraphicsViewModels;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.Graph
{
    public class MSAGLGraphBuilderViewModel : ObservableObject, IGraphTreeBuilderViewModel
    {
        // Collections
        private ObservableCollection<EdgeViewModel> _edges = new();
        public ObservableCollection<EdgeViewModel> Edges { get => _edges; protected set => SetProperty(ref _edges, value); }
        // --
        private readonly ObservableCollection<GraphNodeViewModel> _nodes = new();
        public ReadOnlyObservableCollection<GraphNodeViewModel> Nodes { get; }
        // --
        private readonly Dictionary<Guid, GraphNodeViewModel> _nodesDictionary = new();
        public ReadOnlyDictionary<Guid, GraphNodeViewModel> NodesDictionary { get; }
        public GraphNodeViewModel? GetNodeByUid(Guid uid) => _nodesDictionary.GetValueOrDefault(uid);
        // --


        // Root
        private GraphNodeViewModel? _rootNode;
        public GraphNodeViewModel? RootNode { get => _rootNode; set => SetProperty(ref _rootNode, value); }
        private Dictionary<GraphNodeViewModel, Node> _msaglNodes = new();

        // --
        public System.Drawing.Point RootNodeDefaultPosition { get; } = new System.Drawing.Point(0, 0);
        // --
        public MSAGLGraphBuilderViewModel()
        {
            Nodes = new(_nodes);
            NodesDictionary = new(_nodesDictionary);
        }
        // MSAGL
        public GeometryGraph Graph { get; } = new();
        private Node GetMsaglNodeFrom(GraphNodeViewModel partlyxNode)
        {
            ICurve curve;

            var center = new Point(partlyxNode.Width / 2, partlyxNode.Height / 2);

            switch (partlyxNode.NodeShape)
            {
                case GraphNodeShapeEnum.Circle:
                    curve = CurveFactory.CreateCircle(partlyxNode.Width / 2, center);
                    break;
                default:
                    curve = CurveFactory.CreateRectangle(partlyxNode.Width, partlyxNode.Height, center);
                    break;
            }

            return new Node(curve, partlyxNode);
        }

        private SugiyamaLayoutSettings _defaultSettings => new()
        {
            NodeSeparation = GraphNodeViewModel.StandardNodeDistanceX,
            LayerSeparation = GraphNodeViewModel.StandardNodeDistanceY,
            EdgeRoutingSettings = new() 
            {
                EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.SugiyamaSplines,
            }
        };

        // Other
        public bool AddNode(GraphNodeViewModel node)
        {
            if (!_nodesDictionary.ContainsKey(node.Uid))
            {
                _nodesDictionary.Add(node.Uid, node);
                _nodes.Add(node);

                var msaglNode = GetMsaglNodeFrom(node);
                _msaglNodes.Add(node, msaglNode);
                Graph.Nodes.Add(msaglNode);

                OnNodeAdded(node);

                return true;
            }

            return false;
        }

        public bool RemoveNode(GraphNodeViewModel node)
        {
            if (_nodesDictionary.Remove(node.Uid))
            {
                _nodes.Remove(node);
                if (node is IDisposable d)
                    d.Dispose();

                var msaglNode = _msaglNodes[node];
                _msaglNodes.Remove(node);
                Graph.Nodes.Remove(msaglNode);

                OnNodeRemoved(node);

                return true;
            }

            return false;
        }

        public void BuildEdges()
        {
            Graph.Edges.Clear();

            if (RootNode == null)
                return;

            HashSet<GraphNodeViewModel> visitedNodes = new();
            BuildForRelatives(RootNode);

            void BuildForRelatives(GraphNodeViewModel node)
            {
                visitedNodes.Add(node);

                var thisMsaglode = _msaglNodes.GetValueOrDefault(node);

                if (thisMsaglode == null)
                    return;

                foreach (GraphNodeViewModel parent in node.Parents)
                {
                    if (visitedNodes.Contains(parent))
                        continue;

                    var parentMsaglNode = _msaglNodes.GetValueOrDefault(parent);

                    Graph.Edges.Add(new Edge(parentMsaglNode, thisMsaglode));
                    BuildForRelatives(parent);
                }

                foreach (GraphNodeViewModel child in node.Children)
                {
                    if (visitedNodes.Contains(child))
                        continue;

                    var childMsaglNode = _msaglNodes.GetValueOrDefault(child);

                    Graph.Edges.Add(new Edge(thisMsaglode, childMsaglNode));
                    BuildForRelatives(child);
                }
            }
        }

        public void BuildLayout()
        {
            var settings = _defaultSettings;

            var layout = new LayeredLayout(Graph, settings);
            layout.Run();

            Graph.UpdateBoundingBox();

            foreach (var node in Graph.Nodes)
            {
                var partlyxNode = (GraphNodeViewModel)node.UserData;
                partlyxNode.XCentered = (float)node.Center.X;
                partlyxNode.YCentered = -(float)node.Center.Y;
            }

            foreach (var edge in Graph.Edges)
            {
                edge.SourcePort = new FloatingPort(edge.Source.BoundaryCurve, edge.Source.Center);
                edge.TargetPort = new FloatingPort(edge.Target.BoundaryCurve, edge.Target.Center);
                var geometry = edge.ToPartlyxGeometry();
                var partlyxEdge = new EdgeViewModel(geometry);
                _edges.Add(partlyxEdge);
            }
        }

        public void ClearGraph()
        {
            _nodes.ClearAndTryDispose();
            _nodesDictionary.Clear();
            Edges.Clear();

            Graph.Nodes.Clear();
            Graph.Edges.Clear();

            RootNode = null;

            OnTreeCleared();
        }

        protected virtual void OnNodeAdded(GraphNodeViewModel node) { }
        protected virtual void OnNodeRemoved(GraphNodeViewModel node) { }
        protected virtual void OnTreeCleared() { }
    }
}
