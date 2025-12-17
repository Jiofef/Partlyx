using DynamicData;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface IResourcesVMContainer
    {
        ObservableCollection<ResourceViewModel> Resources { get; }
        SourceList<ResourceViewModel> ResourcesSourceList { get; }

        void AddResource(ResourceViewModel resource);
        void ClearResources();
        void RemoveResource(ResourceViewModel resource);
    }

    public interface IIsolatedResourcesVMContainer : IResourcesVMContainer { }

    public interface IGlobalResourcesVMContainer : IResourcesVMContainer{ }
}