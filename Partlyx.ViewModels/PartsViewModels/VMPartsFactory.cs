using Microsoft.Extensions.DependencyInjection;
using Partlyx.Services.Dtos;

namespace Partlyx.ViewModels.PartsViewModels
{
    public class VMPartsFactory : IVMPartsFactory
    {
        private IServiceProvider _provider;
        public VMPartsFactory(IServiceProvider provider) => _provider = provider;

        public ResourceItemViewModel CreateResourceVM(ResourceDto dto)
        {
            return CreateViewModelFrom<ResourceItemViewModel>(dto);
        }

        public RecipeItemViewModel CreateRecipeVM(RecipeDto dto)
        {
            return CreateViewModelFrom<RecipeItemViewModel>(dto);
        }

        public RecipeComponentItemViewModel CreateRecipeComponentVM(RecipeComponentDto dto)
        {
            return CreateViewModelFrom<RecipeComponentItemViewModel>(dto);
        }

        private TViewModel CreateViewModelFrom<TViewModel>(params object[] args)
        {
            return (TViewModel)ActivatorUtilities.CreateInstance(_provider, typeof(TViewModel), args);
        }
    }
}
