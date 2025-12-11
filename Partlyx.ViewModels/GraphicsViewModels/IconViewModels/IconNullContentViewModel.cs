namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public class IconNullContentViewModel : IIconContentViewModel
    {
        public IconTypeEnumViewModel ContentIconType => IconTypeEnumViewModel.Null;
        public bool IsEmpty => true;
        public bool IsIdentical(IIconContentViewModel other) => true;
    }
}
