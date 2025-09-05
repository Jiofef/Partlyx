using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core;
using Partlyx.Infrastructure.Data;

namespace Partlyx.Services.Commands.RecipeCommonCommands
{
    public class CreateRecipeCommand : IUndoableCommand
    {
        private IRecipeService _recipeService;
        private IResourceRepository _resourceRepository;

        private Guid _resourceUid;
        public Guid RecipeUid { get; private set; }

        private Recipe? _createdRecipe;

        public CreateRecipeCommand(Guid parentResourceUid, IRecipeService rs, IResourceRepository rr)
        {
            _recipeService = rs;
            _resourceUid = parentResourceUid;
        }

        public async Task ExecuteAsync()
        {
            Guid uid = await _recipeService.CreateRecipeAsync(_resourceUid);
            RecipeUid = uid;
            var resource = await _resourceRepository.GetByUidAsync(_resourceUid);
            _createdRecipe = resource?.GetRecipeByUid(uid);
        }

        public async Task UndoAsync()
        {
            await _recipeService.DeleteRecipeAsync(_resourceUid, RecipeUid);
            RecipeUid = Guid.Empty;
        }

        public async Task RedoAsync()
        {
            if (_createdRecipe == null) return;

            await _resourceRepository.ExecuteOnResourceAsync(_resourceUid,
                (resource) =>
                {
                    _createdRecipe.AttachTo(resource);
                    return Task.CompletedTask;
                });
        }
    }

    public class DeleteRecipeCommand : IUndoableCommand
    {
        private IRecipeService _recipeService;
        private IResourceRepository _resourceRepository;

        private Guid _resourceUid;
        public Guid RecipeUid { get; private set; }

        private Recipe? _deletedRecipe;

        public DeleteRecipeCommand(Guid parentResourceUid, Guid recipeUid, IRecipeService rs, IResourceRepository rr)
        {
            _resourceUid = parentResourceUid;
            _recipeService = rs;
            RecipeUid = recipeUid;
            _resourceRepository = rr;
        }

        public async Task ExecuteAsync()
        {
            var resource = await _resourceRepository.GetByUidAsync(_resourceUid);

            if (resource == null) 
                throw new ArgumentNullException(nameof(resource));

            _deletedRecipe = resource.GetRecipeByUid(RecipeUid);

            await _recipeService.DeleteRecipeAsync(_resourceUid, RecipeUid);
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

        public Guid DuplicateUid { get; private set; }

        public DuplicateRecipeCommand(Guid parentResourceUid, Guid recipeUid, IRecipeService rs)
        {
            _recipeService = rs;
            _resourceUid = parentResourceUid;
        }

        public async Task ExecuteAsync()
        {
            DuplicateUid = await _recipeService.DuplicateRecipeAsync(_resourceUid, _recipeUid);
        }

        public async Task UndoAsync()
        {
            await _recipeService.DeleteRecipeAsync(_resourceUid, DuplicateUid);
            DuplicateUid = Guid.Empty;
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
