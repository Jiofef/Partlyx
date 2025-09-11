using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.ServiceInterfaces;
using System.Windows.Input;
namespace Partlyx.ViewModels
{
    public class RecipeItemUIState : ObservableObject
    {
        private readonly IRecipeService _recipeService;

        private readonly Guid _uid;
        private readonly Guid _parentUid;
        public RecipeItemUIState(Guid uid, Guid parentUid) 
        {
            _uid = uid;
            _parentUid = parentUid;
        }

        private bool isSelected;
        private bool isRenaming;
        private string _unConfirmedName;

        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
        public bool IsRenaming { get => isRenaming; set => SetProperty(ref isRenaming, value); }
        public string UnConfirmedName { get => _unConfirmedName; set => SetProperty(ref _unConfirmedName, value); }

        public ICommand CommitNameChangeCommand => new RelayCommand(() =>
        {
            //_recipeService.SetNameAsync(_parentUid, _uid, UnConfirmedName);
        });
    }
}
