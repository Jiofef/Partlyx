using Microsoft.Extensions.DependencyInjection;
using Partlyx.Services.Dtos;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
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

        public ResourceItemViewModel GetOrCreateResourceVM(ResourceDto dto)
        {
            ResourceItemViewModel? resource = _store.Resources.GetValueOrDefault(dto.Uid);
            if (resource != null)
                return resource;

            resource = CreateViewModelFrom<ResourceItemViewModel>(dto);
            _store.Register(resource);
            return resource;
        }

        public RecipeItemViewModel GetOrCreateRecipeVM(RecipeDto dto)
        {
            RecipeItemViewModel? recipe = _store.Recipes.GetValueOrDefault(dto.Uid);
            if (recipe != null)
                return recipe;

            recipe = CreateViewModelFrom<RecipeItemViewModel>(dto);
            _store.Register(recipe);
            return recipe;
        }

        public RecipeComponentItemViewModel GetOrCreateRecipeComponentVM(RecipeComponentDto dto)
        {
            RecipeComponentItemViewModel? component = _store.RecipeComponents.GetValueOrDefault(dto.Uid);
            if (component != null)
                return component;

            component = CreateViewModelFrom<RecipeComponentItemViewModel>(dto);
            _store.Register(component);
            return component;
        }

        private TViewModel CreateViewModelFrom<TViewModel>(params object[] args)
        {
            return (TViewModel)ActivatorUtilities.CreateInstance(_provider, typeof(TViewModel), args);
        }
    }
}
