using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public partial class GuidLinkedPart<TPart> : ObservableObject, IDisposable, IGuidLinkedObject<TPart> where TPart : IVMPart?
    {
        public GuidLinkedPart(Guid Uid) 
        {
            _uid = Uid;
        }

        private Guid _uid;
        public Guid Uid { get => _uid; set => SetProperty(ref _uid, value); }

        private TPart? _value;
        public TPart? Value { get => _value; set => SetProperty(ref _value, value); }

        public event Action Disposed = delegate { };
        public void Dispose()
        {
            Disposed?.Invoke();
        }
    }
}
