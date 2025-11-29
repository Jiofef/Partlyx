using Partlyx.ViewModels.UIObjectViewModels;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public interface IIconVectorCatalog
    {
        public string LibraryName { get; }
        IReadOnlyList<string> GetAllIconKeys();
        IReadOnlyList<StoreVectorIconContentViewModel> GetAllIconsContentForStore(bool returnCachedIfPossible = true);
        IReadOnlyList<string> GetBaseIconKeys();
        IReadOnlyList<StoreVectorIconContentViewModel> GetBaseIconsContentForStore(bool returnCachedIfPossible = true);
    }
}
