using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface IResourcesVMContainer
    {
        ObservableCollection<ResourceViewModel> Resources { get; }
    }

    public interface IIsolatedResourcesVMContainer : IResourcesVMContainer { }

    public interface IGlobalResourcesVMContainer : IResourcesVMContainer{ }
}