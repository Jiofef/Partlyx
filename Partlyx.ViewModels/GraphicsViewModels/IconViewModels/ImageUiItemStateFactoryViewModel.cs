using Microsoft.Extensions.DependencyInjection;
using Partlyx.ViewModels.UIStates;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public class ImageUiItemStateFactoryViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        public ImageUiItemStateFactoryViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        private readonly Dictionary<Guid, ImageUiItemStateViewModel> _states = new();

        public ImageUiItemStateViewModel GetOrCreateItemUi(ImageViewModel vm)
        {
            var state = _states.GetValueOrDefault(vm.Uid);
            if (state == null)
            {
                state = (ImageUiItemStateViewModel)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(ImageUiItemStateViewModel), vm);
                _states.Add(vm.Uid, state);
            }
            return state;
        }
    }
}
