namespace Partlyx.ViewModels.ItemProperties
{
    public class CheckBoxItemPropertyViewModel : ItemPropertyViewModel
    {
        private bool _isChecked;
        public bool IsChecked { get => _isChecked; set { SetProperty(ref _isChecked, value); } }
    }
}
