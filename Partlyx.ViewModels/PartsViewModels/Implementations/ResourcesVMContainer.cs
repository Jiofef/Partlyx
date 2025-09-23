using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public class ResourcesVMContainer : IResourcesVMContainer, IIsolatedResourcesVMContainer, IGlobalResourcesVMContainer
    {
        public ResourcesVMContainer() { }

        public ObservableCollection<ResourceItemViewModel> Resources { get; } = new();
    }
}
