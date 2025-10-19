using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.Services.Commands.RecipeCommonCommands
{
    public class CreateRecipeCommand : IUndoableCommand
    {
        private readonly IRecipeService _recipeService;
        private readonly IResourceService _resourceService;
        private readonly IPartlyxRepository _resourceRepository;
        private readonly IEventBus _bus;

        private Guid _resourceUid;
        public Guid RecipeUid { get; private set; }

        private Recipe? _createdRecipe;
        private string? _recipeName;

        public CreateRecipeCommand(Guid parentResourceUid, IRecipeService rs, IResourceService rs2, IPartlyxRepository rr, IEventBus bus, string? recipeName = null)
        {
            _recipeService = rs;
            _resourceService = rs2;
            _resourceRepository = rr;
            _resourceUid = parentResourceUid;
            _bus = bus;

            _recipeName = recipeName;
        }

        public async Task ExecuteAsync()
        {
            Guid uid = await _recipeService.CreateRecipeAsync(_resourceUid, _recipeName);
            RecipeUid = uid;
            var resource = await _resourceRepository.GetResourceByUidAsync(_resourceUid);
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

            RecipeUid = _createdRecipe.Uid;

            var @event = new RecipeCreatedEvent(_createdRecipe.ToDto());
            await _bus.PublishAsync(@event);

            var resource = await _resourceService.GetResourceAsync(_resourceUid);
            if (resource?.Recipes.Count == 1)
                await _resourceService.SetDefaultRecipeAsync(_resourceUid, RecipeUid);
        }
    }

    public class DeleteRecipeCommand : IUndoableCommand
    {
        private readonly IRecipeService _recipeService;
        private readonly IResourceService _resourceService;
        private readonly IPartlyxRepository _resourceRepository;
        private readonly IEventBus _bus;

        private Guid _resourceUid;
        public Guid RecipeUid { get; private set; }

        private Recipe? _deletedRecipe;

        public DeleteRecipeCommand(Guid parentResourceUid, Guid recipeUid, IRecipeService rs, IResourceService rs2, IPartlyxRepository rr , IEventBus bus)
        {
            _recipeService = rs;
            _resourceService = rs2;
            _resourceRepository = rr;

            _resourceUid = parentResourceUid;
            RecipeUid = recipeUid;
            _bus = bus;
        }

        public async Task ExecuteAsync()
        {
            var resource = await _resourceRepository.GetResourceByUidAsync(_resourceUid);

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

            var @event = new RecipeCreatedEvent(_deletedRecipe.ToDto());
            _bus.Publish(@event);

            _deletedRecipe = null;

            var resource = await _resourceService.GetResourceAsync(_resourceUid);
            if (resource?.Recipes.Count == 1)
                await _resourceService.SetDefaultRecipeAsync(_resourceUid, RecipeUid);
        }
    }

    public class MoveRecipeCommand : IUndoableCommand
    {
        private readonly IRecipeService _recipeService;

        private Guid _resourceUid;
        private Guid _newResourceUid;
        private Guid _recipeUid;

        public MoveRecipeCommand(Guid parentResourceUid, Guid newParentResourceUid, Guid recipeUid, IRecipeService rs)
        {
            _recipeService = rs;
            _resourceUid = parentResourceUid;
            _newResourceUid = newParentResourceUid;
            _recipeUid = recipeUid;
        }

        public async Task ExecuteAsync()
        {
            await _recipeService.MoveRecipeAsync(_resourceUid, _newResourceUid, _recipeUid);
        }

        public async Task UndoAsync()
        {
            await _recipeService.MoveRecipeAsync(_newResourceUid, _resourceUid, _recipeUid);
        }
    }

    public class DuplicateRecipeCommand : IUndoableCommand
    {
        private readonly IRecipeService _recipeService;

        private Guid _resourceUid;
        private Guid _recipeUid;

        public Guid DuplicateUid { get; private set; }

        public DuplicateRecipeCommand(Guid parentResourceUid, Guid recipeUid, IRecipeService rs)
        {
            _recipeService = rs;
            _resourceUid = parentResourceUid;
            _recipeUid = recipeUid;
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

    public class SetRecipeNameCommand : SetValueUndoableCommand<string>
    {
        private SetRecipeNameCommand(string name, string savedValue, Func<string, Task> setter)
            : base(name, savedValue, setter!) { }

        public static async Task<SetRecipeNameCommand?> CreateAsync(IServiceProvider serviceProvider, Guid parentResourceUid, Guid recipeUid, string name)
        {
            var recipeService = serviceProvider.GetRequiredService<IRecipeService>();
            var recipe = await recipeService.GetRecipeAsync(parentResourceUid, recipeUid);
            if (recipe == null)
                throw new ArgumentException("Recipe not found with Uid: " + recipeUid);

            return new SetRecipeNameCommand(name, recipe.Name, async (value) =>
            {
                await recipeService.SetRecipeNameAsync(parentResourceUid, recipeUid, value);
            });
        }
    }
}
