using Partlyx.Core.Partlyx;
using Partlyx.Core.VisualsInfo;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.CoreExtensions;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.Services.ServiceImplementations
{
    public class RecipeService : IRecipeService
    {
        private readonly IPartlyxRepository _repo;
        private readonly IEventBus _eventBus;
        private readonly PartsCreatorService _creator;

        public RecipeService(IPartlyxRepository repo, IEventBus bus)
        {
            _repo = repo;
            _eventBus = bus;
            _creator = new PartsCreatorService(repo, bus);
        }

        public async Task<Guid> CreateRecipeAsync(RecipeCreatingOptions? opt = null)
        {
            if (opt == null)
                opt = new(); // Default options

            bool isReferenceResourceProvided = opt.ReferenceResource != null;

            // Name
            string recipeName = "Recipe";

            if (!opt.OverrideReferenceResourceName && opt.ReferenceResource?.Name != null)
                recipeName = opt.ReferenceResource.Name;
            else if (opt.RecipeName != null) // When overriding the resource name or resource isn't provided
                recipeName = opt.RecipeName;

            recipeName = await _repo.GetUniqueRecipeNameAsync(recipeName);

            var recipe = Recipe.Create(recipeName);

            // Icon
            IIcon icon;
            if (isReferenceResourceProvided)
            {
                icon = new InheritedIcon(opt.ReferenceResource!.Uid, InheritedIcon.InheritedIconParentTypeEnum.Resource);
            }
            else
            {
                icon = new NullIcon();
            }
                var iconInfo = icon.GetInfo();
            recipe.UpdateIconInfo(iconInfo);

            return await _creator.CreateRecipeAsync(recipe);
        }

        public async Task<bool> IsRecipeExists(Guid recipeUid)
            => await _repo.GetRecipeByUidAsync(recipeUid) != null;

        public async Task<Guid> DuplicateRecipeAsync(Guid recipeUid)
        {
            var result = await _repo.DuplicateRecipeAsync(recipeUid);

            var recipe = await _repo.GetRecipeByUidAsync(result);
            if (recipe != null)
            {
                var recipeDto = recipe.ToDto();
                _eventBus.Publish(new RecipeCreatedEvent(recipeDto, recipe.Uid));
            }

            return result;
        }

        public async Task DeleteRecipeAsync(Guid recipeUid)
        {
            await _repo.DeleteRecipeAsync(recipeUid);

            _eventBus.Publish(new RecipeDeletedEvent(recipeUid,
                new HashSet<object>() { recipeUid }));
        }

        public async Task QuantifyRecipeAsync(Guid recipeUid)
        {
            await _repo.ExecuteOnRecipeAsync(recipeUid, recipe =>
            {
                recipe.MakeQuantified();
                return Task.CompletedTask;
            });
        }

        public async Task<RecipeDto?> GetRecipeAsync(Guid recipeUid)
        {
            var recipe = await _repo.GetRecipeByUidAsync(recipeUid);
            return recipe?.ToDto();
        }

        public async Task<List<RecipeDto>> GetAllTheRecipesAsync()
        {
            var recipes = await _repo.GetAllTheRecipesAsync();
            return recipes.Select(r => r.ToDto()).ToList();
        }

        public async Task SetRecipeNameAsync(Guid recipeUid, string name)
        {
            await _repo.ExecuteOnRecipeAsync(recipeUid, recipe =>
            {
                recipe.Name = name;
                return Task.CompletedTask;
            });

            var recipe = await GetRecipeAsync(recipeUid);
            if (recipe != null)
                _eventBus.Publish(new RecipeUpdatedEvent(recipe, new[] { "Name" }, recipe.Uid));
        }

        public async Task SetRecipeIconAsync(Guid recipeUid, IconDto iconDto)
        {
            var iconInfo = iconDto.ToIconInfo();
            await _repo.ExecuteOnRecipeAsync(recipeUid, recipe =>
            {
                recipe.UpdateIconInfo(iconInfo);
                return Task.CompletedTask;
            });

            var recipe = await GetRecipeAsync(recipeUid);
            if (recipe != null)
                _eventBus.Publish(new RecipeUpdatedEvent(recipe, new[] { "Icon" }, recipe.Uid));
        }

        public async Task SetRecipeIsReversibleAsync(Guid recipeUid, bool isReversible)
        {
            await _repo.ExecuteOnRecipeAsync(recipeUid, recipe =>
            {
                recipe.IsReversible = isReversible;
                return Task.CompletedTask;
            });

            var recipe = await GetRecipeAsync(recipeUid);
            if (recipe != null)
                _eventBus.Publish(new RecipeUpdatedEvent(recipe, new[] { "IsReversible" }, recipe.Uid));
        }
    }

    public record RecipeCreatingOptions(string? RecipeName = null, bool OverrideReferenceResourceName = false, ResourceDto? ReferenceResource = null);
}
