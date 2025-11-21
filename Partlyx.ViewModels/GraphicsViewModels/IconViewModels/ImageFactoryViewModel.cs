using Microsoft.Extensions.DependencyInjection;
using Partlyx.Services.Dtos;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public class ImageFactoryViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        public ImageFactoryViewModel(IServiceProvider serviceProvider) 
        {
            _serviceProvider = serviceProvider;
        }

        public ImageViewModel CreateImageViewModel(ImageDto dto)
            => (ImageViewModel)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(ImageViewModel), dto);
    }
}
