using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.ViewModels.PartsViewModels.Implementations;
namespace Partlyx.ViewModels
{
    public partial class RecipeComponentUIState : ObservableObject
    {
        private readonly ICommandServices _commands;

        private readonly RecipeComponentItemViewModel _componentVM;

        public RecipeComponentUIState(RecipeComponentItemViewModel vm, ICommandServices cs)
        {
            _commands = cs;

            _componentVM = vm;
        }

        private bool isSelected;

        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
    }
}
