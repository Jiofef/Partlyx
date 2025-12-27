using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceImplementations;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.Services.Commands.RecipeCommonCommands
{
    public class CreateRecipeCommand : IUndoableCommand
    {
        private readonly IRecipeService _recipeService;
        private readonly PartsCreatorService _partsCreator;
        private readonly IPartlyxRepository _repo;


        public Guid RecipeUid { get; private set; }

        private Recipe? _createdRecipe;
        private string? _recipeName;


        public CreateRecipeCommand(IRecipeService rs, IEventBus bus, IPartlyxRepository repo, string? recipeName = null)
        {
            _recipeService = rs;
            _partsCreator = new PartsCreatorService(repo, bus);

            _recipeName = recipeName;
        }

        public async Task ExecuteAsync()
        {
            RecipeUid = await _recipeService.CreateRecipeAsync(_recipeName);
            // Get the created entity for potential redo
            _createdRecipe = await _repo.GetRecipeByUidAsync(RecipeUid);
        }

        public async Task UndoAsync()
        {
            await _recipeService.DeleteRecipeAsync(RecipeUid);
            RecipeUid = Guid.Empty;
        }

        public async Task RedoAsync()
        {
            if (_createdRecipe != null)
            {
                // Use PartsCreatorService to recreate the exact same recipe entity
                RecipeUid = await _partsCreator.CreateRecipeAsync(_createdRecipe);
            }
        }
    }

    public class DeleteRecipeCommand : IUndoableCommand
    {
        private readonly IRecipeService _recipeService;
        private readonly IPartlyxRepository _repo;
        private readonly PartsCreatorService _partsCreatorService;
        private readonly IEventBus _bus;

        public Guid RecipeUid { get; }

        private Recipe? _deletedRecipe;

        public DeleteRecipeCommand(Guid recipeUid, IRecipeService rs, IEventBus bus, IPartlyxRepository repo)
        {
            RecipeUid = recipeUid;
            _recipeService = rs;
            _bus = bus;
            _repo = repo;

            _partsCreatorService = new(repo, bus);
        }

        public async Task ExecuteAsync()
        {
            _deletedRecipe = await _repo.GetRecipeByUidAsync(RecipeUid);
            await _recipeService.DeleteRecipeAsync(RecipeUid);
        }

        public async Task UndoAsync()
        {
            if (_deletedRecipe != null)
            {
                await _partsCreatorService.CreateRecipeAsync(_deletedRecipe);
                _deletedRecipe = null;
            }
        }
    }

    public class DuplicateRecipeCommand : IUndoableCommand
    {
        private readonly IRecipeService _recipeService;

        public Guid OriginalRecipeUid { get; }
        public Guid DuplicateUid { get; private set; }

        public DuplicateRecipeCommand(Guid recipeUid, IRecipeService rs)
        {
            OriginalRecipeUid = recipeUid;
            _recipeService = rs;
        }

        public async Task ExecuteAsync()
        {
            DuplicateUid = await _recipeService.DuplicateRecipeAsync(OriginalRecipeUid);
        }

        public async Task UndoAsync()
        {
            await _recipeService.DeleteRecipeAsync(DuplicateUid);
            DuplicateUid = Guid.Empty;
        }
    }

    public class SetRecipeNameCommand : SetValueUndoableCommand<string>
    {
        private SetRecipeNameCommand(string name, string savedValue, Func<string, Task> setter)
            : base(name, savedValue, setter) { }

        public static async Task<SetRecipeNameCommand> CreateAsync(IServiceProvider sp, Guid recipeUid, string name)
        {
            var recipeService = sp.GetRequiredService<IRecipeService>();
            var recipe = await recipeService.GetRecipeAsync(recipeUid);
            if (recipe == null)
                throw new ArgumentException("Recipe not found with Uid: " + recipeUid);

            var setter = CreateSetter<IRecipeService, string>(sp, recipeUid, (s, uid, val) => s.SetRecipeNameAsync(uid, val));
            return new SetRecipeNameCommand(name, recipe.Name, setter);
        }
    }

    public class SetRecipeIconCommand : SetValueUndoableCommand<IconDto>
    {
        private SetRecipeIconCommand(IconDto icon, IconDto savedValue, Func<IconDto, Task> setter)
            : base(icon, savedValue, setter) { }

        public static async Task<SetRecipeIconCommand> CreateAsync(IServiceProvider sp, Guid recipeUid, IconDto icon)
        {
            var recipeService = sp.GetRequiredService<IRecipeService>();
            var recipe = await recipeService.GetRecipeAsync(recipeUid);
            if (recipe == null)
                throw new ArgumentException("Recipe not found with Uid: " + recipeUid);

            var setter = CreateSetter<IRecipeService, IconDto>(sp, recipeUid, (s, uid, val)
                => s.SetRecipeIconAsync(uid, val));
            return new SetRecipeIconCommand(icon, recipe.Icon, setter);
        }
    }

    public class SetRecipeIsReversibleCommand : SetValueUndoableCommand<bool>
    {
        private SetRecipeIsReversibleCommand(bool isReversible, bool savedValue, Func<bool, Task> setter)
            : base(isReversible, savedValue, setter) { }

        public static async Task<SetRecipeIsReversibleCommand> CreateAsync(IServiceProvider sp, Guid recipeUid, bool isReversible)
        {
            var recipeService = sp.GetRequiredService<IRecipeService>();
            var recipe = await recipeService.GetRecipeAsync(recipeUid);
            if (recipe == null)
                throw new ArgumentException("Recipe not found with Uid: " + recipeUid);

            var setter = CreateSetter<IRecipeService, bool>(sp, recipeUid, (s, uid, val)
                => s.SetRecipeIsReversibleAsync(uid, val));
            return new SetRecipeIsReversibleCommand(isReversible, recipe.IsReversible, setter);
        }
    }
}
