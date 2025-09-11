using CommunityToolkit.Mvvm.ComponentModel;
namespace Partlyx.ViewModels
{
    public class RecipeComponentUIState : ObservableObject
    {
        private bool isSelected;

        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
    }
}
