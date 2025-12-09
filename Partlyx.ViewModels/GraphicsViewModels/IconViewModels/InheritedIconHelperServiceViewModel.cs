using Partlyx.Core.VisualsInfo;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public class InheritedIconHelperServiceViewModel
    {
        private readonly IVMPartsStore _store;
        public InheritedIconHelperServiceViewModel(IVMPartsStore store)
        {
            _store = store;
        }
        public async Task<IObservableFindableIconHolder?> GetIconHolderOrNullAsync(Guid uid, InheritedIcon.InheritedIconParentTypeEnum type)
        {
            switch (type)
            {
                case InheritedIcon.InheritedIconParentTypeEnum.Resource:
                    return (ResourceViewModel?) await _store.TryGetAsync(uid);
                case InheritedIcon.InheritedIconParentTypeEnum.Recipe:
                    return (RecipeViewModel?) await _store.TryGetAsync(uid);
                default:
                    return null;
            }
        }
    }
}