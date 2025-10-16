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

        public ResourceViewModel GetOrCreateResourceVM(ResourceDto dto)
        {
            ResourceViewModel? resource = _store.Resources.GetValueOrDefault(dto.Uid);
            if (resource != null)
                return resource;

            resource = CreateViewModelFrom<ResourceViewModel>(dto);
            _store.Register(resource);
            return resource;
        }

        public RecipeViewModel GetOrCreateRecipeVM(RecipeDto dto)
        {
            RecipeViewModel? recipe = _store.Recipes.GetValueOrDefault(dto.Uid);
            if (recipe != null)
                return recipe;

            recipe = CreateViewModelFrom<RecipeViewModel>(dto);
            _store.Register(recipe);
            return recipe;
        }

        public RecipeComponentViewModel GetOrCreateRecipeComponentVM(RecipeComponentDto dto)
        {
            RecipeComponentViewModel? component = _store.RecipeComponents.GetValueOrDefault(dto.Uid);
            if (component != null)
                return component;

            component = CreateViewModelFrom<RecipeComponentViewModel>(dto);
            _store.Register(component);
            return component;
        }

        private TViewModel CreateViewModelFrom<TViewModel>(params object[] args)
        {
            return (TViewModel)ActivatorUtilities.CreateInstance(_provider, typeof(TViewModel), args);
        }
    }
}
