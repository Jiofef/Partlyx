namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IResourceItemUiStateService
    {
        ResourceItemUIState GetOrCreate(Guid id);
    }
}