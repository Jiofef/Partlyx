using DynamicData;
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

        public ObservableCollection<ResourceViewModel> Resources { get; } = new();
        public SourceList<ResourceViewModel> ResourcesSourceList { get; } = new();

        public void ClearResources()
        {
            ResourcesSourceList.Clear();
            Resources.Clear();
        }
        public void AddResource(ResourceViewModel resource)
        {
            ResourcesSourceList.Add(resource);
            Resources.Add(resource);
        }
        public void RemoveResource(ResourceViewModel resource)
        {
            ResourcesSourceList.Remove(resource);
            Resources.Remove(resource);
        }
    }
}
