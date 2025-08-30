using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core;
using Partlyx.Data;

namespace Partlyx.Services.Commands.RecipeCommonCommands
{
    public class CreateRecipeCommand : IUndoableCommand
    {
        private IRecipeService _recipeService;

        private Guid _resourceUid;
        private Guid _recipeUid;

        public CreateRecipeCommand(IServiceProvider serviceProvider, Guid parentResourceUid)
        {
            _recipeService = serviceProvider.GetRequiredService<IRecipeService>();
            _resourceUid = parentResourceUid;
        }

        public async Task ExecuteAsync()
        {
            Guid uid = await _recipeService.CreateRecipeAsync(_resourceUid);
            _recipeUid = uid;
        }

        public async Task UndoAsync()
        {
            await _recipeService.DeleteRecipeAsync(_resourceUid, _recipeUid);
            _recipeUid = Guid.Empty;
        }
    }

    public class DeleteRecipeCommand : IUndoableCommand
    {
        private IRecipeService _recipeService;
        private IResourceRepository _resourceRepository;

        private Guid _resourceUid;
        private Guid _recipeUid;

        private Recipe? _deletedRecipe;

        public DeleteRecipeCommand(IServiceProvider serviceProvider, Guid parentResourceUid, Guid recipeUid)
        {
            _resourceUid = parentResourceUid;
            _recipeService = serviceProvider.GetRequiredService<IRecipeService>();
            _recipeUid = recipeUid;
            _resourceRepository = serviceProvider.GetRequiredService<IResourceRepository>();
        }

        public async Task ExecuteAsync()
        {
            var resource = await _resourceRepository.GetByUidAsync(_resourceUid);

            if (resource == null) 
                throw new ArgumentNullException(nameof(resource));

            _deletedRecipe = resource.GetRecipeByUid(_recipeUid);

            await _recipeService.DeleteRecipeAsync(_resourceUid, _recipeUid);
        }

        public async Task UndoAsync()
        {
            if (_deletedRecipe == null) return;

            await _resourceRepository.ExecuteOnResourceAsync(_resourceUid,
                (resource) => 
                {
                    _deletedRecipe.AttachTo(resource);
                    return Task.CompletedTask;
                }); 

            _deletedRecipe = null;
        }
    }

    public class DuplicateRecipeCommand : IUndoableCommand
    {
        private IRecipeService _recipeService;

        private Guid _resourceUid;
        private Guid _recipeUid;

        private Guid _duplicateUid;

        public DuplicateRecipeCommand(IServiceProvider serviceProvider, Guid parentResourceUid, Guid recipeUid)
        {
            _recipeService = serviceProvider.GetRequiredService<IRecipeService>();
            _resourceUid = parentResourceUid;
        }

        public async Task ExecuteAsync()
        {
            _duplicateUid = await _recipeService.DuplicateRecipeAsync(_resourceUid, _recipeUid);
        }

        public async Task UndoAsync()
        {
            await _recipeService.DeleteRecipeAsync(_resourceUid, _duplicateUid);
            _duplicateUid = Guid.Empty;
        }
    }

    public class SetRecipeCraftAmountCommand : SetValueUndoableCommand<double>
    {
        private SetRecipeCraftAmountCommand(double amount, double savedValue, Func<double, Task> setter)
            : base(amount, savedValue, setter) { }

        public static async Task<SetRecipeCraftAmountCommand?> CreateAsync(IServiceProvider serviceProvider, Guid parentResourceUid, Guid recipeUid, double amount)
        {
            var recipeService = serviceProvider.GetRequiredService<IRecipeService>();
            var recipe = await recipeService.GetRecipeAsync(parentResourceUid, recipeUid);
            if (recipe == null)
                throw new ArgumentException("Recipe not found with Uid: " + recipeUid);

            return new SetRecipeCraftAmountCommand(amount, recipe.CraftAmount, async (value) =>
            {
                await recipeService.SetRecipeCraftAmountAsync(parentResourceUid, recipeUid, value);
            });
        }
    }
}
