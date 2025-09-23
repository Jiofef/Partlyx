using Microsoft.Extensions.DependencyInjection;
using Partlyx.ViewModels.UIServices.Interfaces;

namespace Partlyx.UI.WPF.VMImplementations
{
    public class DialogService : IDialogService
    {
        private readonly IServiceProvider _services;

        public DialogService(IServiceProvider rootProvider) => _services = rootProvider;

        public async Task<object?> ShowDialogAsync<TViewModel>(string hostIdentifier = "RootDialog") where TViewModel : class
        {
            using var scope = _services.CreateScope();
            var vm = scope.ServiceProvider.GetRequiredService<TViewModel>();
            try
            {
                var result = await MaterialDesignThemes.Wpf.DialogHost.Show(vm, hostIdentifier);
                return result;
            }
            finally { }
        }

        public Task<object?> ShowDialogAsync(object viewModel, string hostIdentifier = "RootDialog")
            => MaterialDesignThemes.Wpf.DialogHost.Show(viewModel, hostIdentifier);
    }
}
