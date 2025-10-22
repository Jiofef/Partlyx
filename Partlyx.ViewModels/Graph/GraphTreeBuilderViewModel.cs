using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.ViewModels.GraphicsViewModels;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Numerics;
using UJL.CSharp.Collections;

namespace Partlyx.ViewModels.Graph
{
    public class GraphTreeBuilderViewModel : ObservableObject, IGraphTreeBuilderViewModel
    {
        // Collections
        protected ObservableMultiCollection<FromToLineViewModel> _edges = new();
        public ObservableMultiCollection<FromToLineViewModel> Edges { get => _edges; protected set => SetProperty(ref _edges, value); }

        private GraphTreeNodeViewModel? _rootNode;
        public GraphTreeNodeViewModel? RootNode { get => _rootNode; protected set => SetProperty(ref _rootNode, value); }

        public ObservableCollection<GraphTreeNodeViewModel> Nodes { get; } = new();

        protected readonly Dictionary<Guid, GraphTreeNodeViewModel> NodesDictionary = new();

        public Point RootNodeDefaultPosition { get; } = new Point(0, 0);

        private Vector2 GetChildPositionFromParentPosition(GraphTreeNodeViewModel child, Vector2 parentPosition)
            => new Vector2(parentPosition.X + child.XLocal, parentPosition.Y + child.YLocal);

        protected void BuildEdgesFor(GraphTreeNodeViewModel node)
        {
            var nodePosition = node.GetPositionCentered();
            BuildEdgesFor(node, nodePosition);
        }
        // To avoid unnecesarry global position chain calculations
        protected void BuildEdgesFor(GraphTreeNodeViewModel node, Vector2 nodePositionCentered)
        {
            node.ConnectedLines.Clear();
            if (node.Children.Count == 0) return;

            if (node.Children.Count == 1)
            {
                var child = (GraphTreeNodeViewModel)node.Children.First();
                var childPosition = GetChildPositionFromParentPosition(child, nodePositionCentered);

                var line = new FromToLineViewModel(nodePositionCentered, childPosition);
                node.ConnectedLines.Add(line);

                BuildEdgesFor(child, childPosition);
            }
            else // if children is two or more
            {
                var firstChildPosition = GetChildPositionFromParentPosition((GraphTreeNodeViewModel)node.Children.First(), nodePositionCentered);
                var lastChildPosition = GetChildPositionFromParentPosition((GraphTreeNodeViewModel)node.Children.Last(), nodePositionCentered);
                float middleHorizontalLineYOffset = nodePositionCentered.Y + node.Height / 2 + node.SingleChildrenDistanceY / 2;

                var lineToHorizontalLine = new FromToLineViewModel(
                    nodePositionCentered,
                    new Vector2(nodePositionCentered.X, middleHorizontalLineYOffset));
                node.ConnectedLines.Add(lineToHorizontalLine);

                var horizontalLine = new FromToLineViewModel(
                    new Vector2(firstChildPosition.X, middleHorizontalLineYOffset),
                    new Vector2(lastChildPosition.X, middleHorizontalLineYOffset));
                node.ConnectedLines.Add(horizontalLine);

                foreach (GraphTreeNodeViewModel child in node.Children)
                {
                    var childPosition = GetChildPositionFromParentPosition(child, nodePositionCentered);

                    var lineFromHorizontalLine = new FromToLineViewModel(
                        new Vector2(childPosition.X, middleHorizontalLineYOffset),
                        childPosition);
                    node.ConnectedLines.Add(lineFromHorizontalLine);

                    BuildEdgesFor(child, childPosition);
                }
            }
        }

        public void DestroyBranch(GraphTreeNodeViewModel branch, bool destroyRoot = true)
        {
            void DestroyChildren(GraphTreeNodeViewModel parent)
            {
                foreach (GraphTreeNodeViewModel child in parent.Children)
                {
                    DestroyChildren(child);
                    RemoveNode(child);
                }
            }

            DestroyChildren(branch);

            if (destroyRoot)
                RemoveNode(branch);
        }

        protected GraphTreeNodeViewModel? GetNodeByUid(Guid uid) => NodesDictionary.GetValueOrDefault(uid);

        public void UpdateTreePositions()
        {
            RootNode?.BuildChildren();
        }

        public void AddNode(GraphTreeNodeViewModel node)
        {
            Nodes.Add(node);
            NodesDictionary.Add(node.Uid, node);

            OnNodeAdded(node);
        }

        public void RemoveNode(GraphTreeNodeViewModel node)
        {
            Nodes.Remove(node);
            NodesDictionary.Remove(node.Uid);

            OnNodeRemoved(node);
        }

        public void DestroyTree()
        {
            Nodes.Clear();
            NodesDictionary.Clear();
            Edges.ClearCollections();

            OnTreeDestroyed();
        }

        protected virtual void OnNodeAdded(GraphTreeNodeViewModel node) { }
        protected virtual void OnNodeRemoved(GraphTreeNodeViewModel node) { }
        protected virtual void OnTreeDestroyed() { }
    }
}
