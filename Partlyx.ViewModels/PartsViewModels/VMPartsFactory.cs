using Microsoft.Extensions.DependencyInjection;
using Partlyx.Services.Dtos;

namespace Partlyx.ViewModels.PartsViewModels
{
    public class VMPartsFactory : IVMPartsFactory
    {
        private IServiceProvider _provider;
        private IVMPartsStore _store;
        public VMPartsFactory(IServiceProvider provider, IVMPartsStore repository)
        {
            _provider = provider;
            _store = repository;
        }

        public ResourceItemViewModel CreateResourceVM(ResourceDto dto)
        {
            var resource = CreateViewModelFrom<ResourceItemViewModel>(dto);
            _store.Register(resource);
            return resource;
        }

        public RecipeItemViewModel CreateRecipeVM(RecipeDto dto)
        {
            var recipe = CreateViewModelFrom<RecipeItemViewModel>(dto);
            _store.Register(recipe);
            return recipe;
        }

        public RecipeComponentItemViewModel CreateRecipeComponentVM(RecipeComponentDto dto)
        {
            var component = CreateViewModelFrom<RecipeComponentItemViewModel>(dto);
            _store.Register(component);
            return component;
        }

        private TViewModel CreateViewModelFrom<TViewModel>(params object[] args)
        {
            return (TViewModel)ActivatorUtilities.CreateInstance(_provider, typeof(TViewModel), args);
        }
    }
}
