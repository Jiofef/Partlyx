using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.UIServices.Interfaces;
using System.Diagnostics;
using Partlyx.ViewModels.UIServices;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class MenuPanelFileViewModel
    {
        private IVMFileService _fileService;

        public MenuPanelFileViewModel(IVMFileService vmfs)
        {
            _fileService = vmfs;
        }

        public bool IsChangesSaved => _fileService.IsChangesSaved;

        [RelayCommand]
        public async Task NewFileAsync()
        {
            await _fileService.NewFileAsync();
        }

        [RelayCommand]
        public async Task<bool> SaveProjectAsync()
        {
            return await _fileService.SaveProjectAsync();
        }

        [RelayCommand]
        public async Task<bool> SaveProjectAsAsync()
        {
            return await _fileService.SaveProjectAsAsync();
        }

        [RelayCommand]
        public async Task OpenProjectAsync()
        {
            await _fileService.OpenProjectAsync();
        }
    }
}
