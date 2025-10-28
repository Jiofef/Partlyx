using Microsoft.Extensions.DependencyInjection;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;

namespace Partlyx.UI.Avalonia.VMImplementations
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
                var result = await MaterialDesignThemes.Avalonia.DialogHost.Show(vm, hostIdentifier);
                return result;
            }
            finally { }
        }

        public Task<object?> ShowDialogAsync(object viewModel, string hostIdentifier = "RootDialog")
            => MaterialDesignThemes.Avalonia.DialogHost.Show(viewModel, hostIdentifier);
    }
}
