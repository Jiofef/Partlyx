namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public interface IIconContentViewModel
    {
        IconTypeEnumViewModel ContentIconType { get; }

        IconViewModel ToIcon()
        {
            return new IconViewModel() { Content = this, IconType = ContentIconType };
        }

        bool IsEmpty { get => false; }

        public bool IsIdentical(IIconContentViewModel other) => false;
    }
}
