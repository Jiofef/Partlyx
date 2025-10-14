using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Implementations;
namespace Partlyx.ViewModels.UIStates
{
    public partial class RecipeComponentUIState : ObservableObject
    {
        private readonly PartsServiceViewModel _services;

        private readonly RecipeComponentItemViewModel _componentVM;

        public RecipeComponentUIState(RecipeComponentItemViewModel vm, PartsServiceViewModel svm)
        {
            _services = svm;

            _componentVM = vm;
        }

        private bool _isSelected;

        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }
}
