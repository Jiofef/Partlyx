using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.ServiceInterfaces;
using System.Windows.Input;
namespace Partlyx.ViewModels
{
    public class ResourceItemUIState : ObservableObject
    {
        private readonly IResourceService _resourceService;
        private readonly Guid _uid;
        public ResourceItemUIState(Guid uid, IResourceService rs)
        {
            _resourceService = rs;

            _uid = uid;
        }

        private bool _isSelected;
        private bool _isRenaming;
        private string _unConfirmedName;

        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
        public bool IsRenaming { get => _isRenaming; set => SetProperty(ref _isRenaming, value); }
        public string UnConfirmedName { get => _unConfirmedName; set => SetProperty(ref _unConfirmedName, value); }

        public ICommand CommitNameChangeCommand => new RelayCommand(() =>
        {
            _resourceService.SetNameAsync(_uid, UnConfirmedName);
        });
    }
}
