using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ReactiveUI;
using System.Text;
using System.Threading.Tasks;
using Partlyx.ViewModels.GraphicsViewModels;

namespace Partlyx.ViewModels.Graph
{
    public class TwoObjectsLineViewModel : FromToLineViewModel, IDisposable
    {
        private readonly IDisposable _fromNodeXChangedSubscription;
        private readonly IDisposable _fromNodeYChangedSubscription;
        private readonly IDisposable _toNodeXChangedSubscription;
        private readonly IDisposable _toNodeYChangedSubscription;
        public TwoObjectsLineViewModel(IPositionObject from, IPositionObject to)
        {
            _fromPositionObj = from;
            _toPositionObj = to;

            From = GetObjectPosition(_fromPositionObj);
            To = GetObjectPosition(_toPositionObj);

            _fromNodeXChangedSubscription = this.WhenAnyValue(x => x.FromNode.X).Subscribe(x => OnFromObjectPositionChanged());
            _fromNodeYChangedSubscription = this.WhenAnyValue(x => x.FromNode.Y).Subscribe(x => OnFromObjectPositionChanged());
            _toNodeXChangedSubscription = this.WhenAnyValue(x => x.ToNode.X).Subscribe(x => OnToObjectPositionChanged());
            _toNodeYChangedSubscription = this.WhenAnyValue(x => x.ToNode.Y).Subscribe(x => OnToObjectPositionChanged());
        }
        public void Dispose()
        {
            _fromNodeXChangedSubscription.Dispose();
            _fromNodeYChangedSubscription.Dispose();
            _toNodeXChangedSubscription.Dispose();
            _toNodeYChangedSubscription.Dispose();
        }
        private void OnFromObjectPositionChanged()
        {
            From = GetObjectPosition(_fromPositionObj);
        }
        private void OnToObjectPositionChanged()
        {
            To = GetObjectPosition(_toPositionObj);
        }

        private IPositionObject _fromPositionObj;
        public IPositionObject FromNode { get => _fromPositionObj; set => SetProperty(ref _fromPositionObj, value); }

        private IPositionObject _toPositionObj;
        public IPositionObject ToNode { get => _toPositionObj; set => SetProperty(ref _toPositionObj, value); }

        private Vector2 GetObjectPosition(IPositionObject obj)
        {
            if (obj is ISizePositionObject spo)
            {
                return new Vector2(spo.XCentered, spo.YCentered);
            }
            else
            {
                return new Vector2(obj.X, obj.Y);
            }
        }
    }
}
