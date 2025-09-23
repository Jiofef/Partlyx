namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IDialogService
    {
        Task<object?> ShowDialogAsync(object dialogViewModel, string hostIdentifier = "RootDialog");
        Task<object?> ShowDialogAsync<TViewModel>(string hostIdentifier = "RootDialog") where TViewModel : class;
    }
}
