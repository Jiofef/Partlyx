using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ReactiveUI;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.Graph
{
    public class EdgeViewModel : FromToLineViewModel, IDisposable
    {
        private readonly IDisposable _fromNodeXChangedSubscription;
        private readonly IDisposable _fromNodeYChangedSubscription;
        private readonly IDisposable _toNodeXChangedSubscription;
        private readonly IDisposable _toNodeYChangedSubscription;
        public EdgeViewModel(GraphNodeViewModel from, GraphNodeViewModel to)
        {
            _fromNode = from;
            _toNode = to;

            From = new Vector2(_fromNode.XCentered, _fromNode.YCentered);
            To = new Vector2(_toNode.XCentered, _toNode.YCentered);

            _fromNodeXChangedSubscription = this.WhenAnyValue(x => x.FromNode.X).Subscribe(x => OnFromNodePositionChanged());
            _fromNodeYChangedSubscription = this.WhenAnyValue(x => x.FromNode.Y).Subscribe(x => OnFromNodePositionChanged());
            _toNodeXChangedSubscription = this.WhenAnyValue(x => x.ToNode.X).Subscribe(x => OnToNodePositionChanged());
            _toNodeYChangedSubscription = this.WhenAnyValue(x => x.ToNode.Y).Subscribe(x => OnToNodePositionChanged());
        }
        public void Dispose()
        {
            _fromNodeXChangedSubscription.Dispose();
            _fromNodeYChangedSubscription.Dispose();
            _toNodeXChangedSubscription.Dispose();
            _toNodeYChangedSubscription.Dispose();
        }
        private void OnFromNodePositionChanged()
        {
            From = new Vector2(_fromNode.XCentered, _fromNode.YCentered);
        }
        private void OnToNodePositionChanged()
        {
            To = new Vector2(_toNode.XCentered, _toNode.YCentered);
        }

        private GraphNodeViewModel _fromNode;
        public GraphNodeViewModel FromNode { get => _fromNode; set => SetProperty(ref _fromNode, value); }

        private GraphNodeViewModel _toNode;
        public GraphNodeViewModel ToNode { get => _toNode; set => SetProperty(ref _toNode, value); }
    }
}
