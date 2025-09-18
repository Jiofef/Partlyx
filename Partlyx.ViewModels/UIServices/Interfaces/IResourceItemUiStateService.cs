using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IResourceItemUiStateService
    {
        ResourceItemUIState GetOrCreate(ResourceItemViewModel vm);
    }
}