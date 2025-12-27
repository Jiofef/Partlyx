using Partlyx.Core.Partlyx;
using Partlyx.Core.VisualsInfo;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.CoreExtensions;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;

namespace Partlyx.Services.ServiceImplementations
{
    /// <summary>
    /// Internal service for creating parts (resources, recipes, components) with proper persistence and event publishing.
    /// Used internally by other services to avoid code duplication.
    /// </summary>
    internal class PartsCreatorService
    {
        private readonly IPartlyxRepository _repo;
        private readonly IEventBus _eventBus;

        public PartsCreatorService(IPartlyxRepository repo, IEventBus eventBus)
        {
            _repo = repo;
            _eventBus = eventBus;
        }

        public async Task<Guid> CreateResourceAsync(Resource resource)
        {
            var uid = await _repo.AddResourceAsync(resource);
            var resourceDto = resource.ToDto();
            _eventBus.Publish(new ResourceCreatedEvent(resourceDto, resource.Uid));
            return uid;
        }

        public async Task<Guid> CreateRecipeAsync(Recipe recipe)
        {
            var uid = await _repo.AddRecipeAsync(recipe);
            var recipeDto = recipe.ToDto();
            _eventBus.Publish(new RecipeCreatedEvent(recipeDto, recipe.Uid));
            return uid;
        }

        public async Task<Guid> CreateRecipeComponentAsync(RecipeComponent component)
        {
            // Note: RecipeComponent doesn't have its own Add method in repo,
            // it's saved when the parent recipe is saved
            // But we still need to publish the event
            var componentDto = component.ToDto();
            _eventBus.Publish(new RecipeComponentCreatedEvent(componentDto, component.Uid));
            return component.Uid;
        }
    }
}
