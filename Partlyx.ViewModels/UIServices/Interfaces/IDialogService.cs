namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IDialogService
    {
        public const string DefaultDialogIdentifier = "RootDialog";

        Task<object?> ShowDialogAsync(object dialogViewModel, string hostIdentifier = DefaultDialogIdentifier);
        Task<object?> ShowDialogAsync<TViewModel>(string hostIdentifier = DefaultDialogIdentifier) where TViewModel : class;

        void Close(string hostIdentifier, object? result = null);
    }
}
