using Avalonia.Controls;
using DialogHostAvalonia;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Threading.Tasks;
namespace Partlyx.UI.Avalonia.VMImplementations
{
    public class DialogService : IDialogService
    {
        public const string DefaultDialogIdentifier = IDialogService.DefaultDialogIdentifier;

        private readonly IServiceProvider _services;
        public DialogService(IServiceProvider rootProvider) => _services = rootProvider;
        public async Task<object?> ShowDialogAsync<TViewModel>(string hostIdentifier = DefaultDialogIdentifier) where TViewModel : class
        {
            using var scope = _services.CreateScope();
            var vm = scope.ServiceProvider.GetRequiredService<TViewModel>();
            return await DialogHost.Show(vm, hostIdentifier);
        }
        public Task<object?> ShowDialogAsync(object viewModel, string hostIdentifier = DefaultDialogIdentifier)
        => DialogHost.Show(viewModel, hostIdentifier);

        public void Close(string hostIdentifier, object? result = null)
        {
            DialogHost.Close(hostIdentifier, result);
        }
    }
}