using Partlyx.ViewModels.PartsViewModels;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IResourceItemUiStateService
    {
        ResourceItemUIState GetOrCreate(ResourceItemViewModel vm);
    }
}