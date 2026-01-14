using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UJL.CSharp.Collections;

namespace Partlyx.ViewModels.Graph.PartsGraph
{
    public class ResourceGraphNodeViewModel : GraphNodeViewModel, IDisposable, ITypedVMPartHolder<ResourceViewModel>
    {
        private readonly IDisposable _valueUpdateSubscription;
        public ResourceGraphNodeViewModel(ResourceViewModel value, GraphNodeViewModel? mainRelative = null)
            : base(mainRelative, value) 
        {
            _resource = value;

            _valueUpdateSubscription =
                this.WhenAnyValue(@this => @this.Value)
                .Subscribe((o) => OnValueChanged());
        }
        private void OnValueChanged()
        {
            if (Value is ResourceViewModel resource)
                Part = resource;
        }
        public PartTypeEnumVM? PartType => PartTypeEnumVM.Resource;
        private ResourceViewModel? _resource = null;
        public ResourceViewModel? Part { get => _resource; private set => SetProperty(ref _resource, value); }

        public void Dispose()
        {
            _valueUpdateSubscription.Dispose();
        }
    }
}
