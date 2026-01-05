using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceImplementations;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.Services.Commands.RecipeComponentCommonCommands
{
    public class CreateRecipeComponentCommand : IUndoableCommand
    {
        private IRecipeComponentService _recipeComponentService;
        private readonly PartsCreatorService _partsCreator;
        private readonly IPartlyxRepository _repo;

        private Guid _recipeUid;
        private Guid _componentResourceUid;

        public Guid RecipeComponentUid { get; private set; }
        public RecipeComponentCreatingOptions? CreatingOptions { get; }

        private RecipeComponent? _createdRecipeComponent;

        public CreateRecipeComponentCommand(Guid parentRecipeUid, Guid componentResourceUid,
            IRecipeComponentService rcs, IEventBus bus, IPartlyxRepository repo, RecipeComponentCreatingOptions? opt = null)
        {
            _recipeComponentService = rcs;
            _partsCreator = new PartsCreatorService(repo, bus);
            _repo = repo;

            _recipeUid = parentRecipeUid;
            _componentResourceUid = componentResourceUid;

            CreatingOptions = opt;
        }

        public async Task ExecuteAsync()
        {
            RecipeComponentUid = CreatingOptions?.IsOutput ?? false
                ? await _recipeComponentService.CreateOutputAsync(_recipeUid, _componentResourceUid, CreatingOptions)
                : await _recipeComponentService.CreateInputAsync(_recipeUid, _componentResourceUid, CreatingOptions);

            // Get the created entity for potential redo
            _createdRecipeComponent = await _repo.GetRecipeComponentByUidAsync(RecipeComponentUid);
        }

        public async Task UndoAsync()
        {
            await _recipeComponentService.DeleteComponentAsync(RecipeComponentUid);
            RecipeComponentUid = Guid.Empty;
        }

        public async Task RedoAsync()
        {
            if (_createdRecipeComponent != null)
            {
                // Use PartsCreatorService to recreate the exact same component entity
                RecipeComponentUid = await _partsCreator.CreateRecipeComponentAsync(_createdRecipeComponent);
            }
        }
    }

    public class DeleteRecipeComponentCommand : IUndoableCommand
    {
        private IRecipeComponentService _recipeComponentService;
        private readonly PartsCreatorService _partsCreator;
        private readonly IPartlyxRepository _repo;

        public Guid RecipeComponentUid { get; }

        private RecipeComponent? _deletedRecipeComponent;

        public DeleteRecipeComponentCommand(Guid recipeComponentUid, IRecipeComponentService rcs, IEventBus bus, IPartlyxRepository repo)
        {
            RecipeComponentUid = recipeComponentUid;
            _recipeComponentService = rcs;
            _partsCreator = new PartsCreatorService(repo, bus);
            _repo = repo;
        }

        public async Task ExecuteAsync()
        {
            _deletedRecipeComponent = await _repo.GetRecipeComponentByUidAsync(RecipeComponentUid);
            await _recipeComponentService.DeleteComponentAsync(RecipeComponentUid);
        }

        public async Task UndoAsync()
        {
            if (_deletedRecipeComponent != null)
            {
                await _partsCreator.CreateRecipeComponentAsync(_deletedRecipeComponent);
                _deletedRecipeComponent = null;
            }
        }
    }

    public class MoveRecipeComponentCommand : IUndoableCommand
    {
        private IRecipeComponentService _recipeComponentService;

        private Guid _recipeUid;
        private Guid _newRecipeUid;
        private Guid _recipeComponentUid;

        public MoveRecipeComponentCommand(Guid parentRecipeUid, Guid newParentRecipeUid, Guid componentUid, IRecipeComponentService rcs)
        {
            _recipeComponentService = rcs;
            _recipeUid = parentRecipeUid;
            _newRecipeUid = newParentRecipeUid;
            _recipeComponentUid = componentUid;
        }

        public async Task ExecuteAsync()
        {
            await _recipeComponentService.MoveComponentAsync(_recipeUid, _newRecipeUid, _recipeComponentUid);
        }

        public async Task UndoAsync()
        {
            await _recipeComponentService.MoveComponentAsync(_newRecipeUid, _recipeUid, _recipeComponentUid);
        }
    }

    public class SetRecipeComponentQuantityCommand : SetValueUndoableCommand<double>
    {
        private SetRecipeComponentQuantityCommand(double quantity, double savedValue, Func<double, Task> setter)
            : base(quantity, savedValue, setter) { }

        public static async Task<SetRecipeComponentQuantityCommand?> CreateAsync(IServiceProvider serviceProvider, Guid recipeComponentUid, double amount)
        {
            var recipeComponentService = serviceProvider.GetRequiredService<IRecipeComponentService>();
            var component = await recipeComponentService.GetComponentAsync(recipeComponentUid);
            if (component == null)
                throw new ArgumentException("Component not found with Uid: " + recipeComponentUid);

            var setter = CreateSetter<IRecipeComponentService, double>(serviceProvider, recipeComponentUid, (s, uid, val) 
                => s.SetQuantityAsync(uid, val));
            return new SetRecipeComponentQuantityCommand(amount, component.Quantity, setter);
        }
    }

    public class DuplicateRecipeComponentCommand : IUndoableCommand
    {
        private IRecipeComponentService _recipeComponentService;

        public Guid OriginalComponentUid { get; }
        public Guid DuplicateUid { get; private set; }

        public DuplicateRecipeComponentCommand(Guid recipeComponentUid, IRecipeComponentService rcs)
        {
            OriginalComponentUid = recipeComponentUid;
            _recipeComponentService = rcs;
        }

        public async Task ExecuteAsync()
        {
            DuplicateUid = await _recipeComponentService.DuplicateComponentAsync(OriginalComponentUid);
        }

        public async Task UndoAsync()
        {
            await _recipeComponentService.DeleteComponentAsync(DuplicateUid);
            DuplicateUid = Guid.Empty;
        }
    }

    public class SetRecipeComponentResourceCommand : SetValueUndoableCommand<Guid>
    {
        private SetRecipeComponentResourceCommand(Guid newResource, Guid previousResource, Func<Guid, Task> setter)
            : base(newResource, previousResource, setter) { }

        public static async Task<SetRecipeComponentResourceCommand?> CreateAsync(IServiceProvider serviceProvider, Guid recipeComponentUid, Guid newSelectedResourceUid)
        {
            var recipeComponentService = serviceProvider.GetRequiredService<IRecipeComponentService>();
            var component = await recipeComponentService.GetComponentAsync(recipeComponentUid);
            if (component == null)
                throw new ArgumentException("Component not found with Uid: " + recipeComponentUid);

            var setter = CreateSetter<IRecipeComponentService, Guid>(serviceProvider, recipeComponentUid, (s, uid, val) 
                => s.SetComponentResourceAsync(uid, val));
            return new SetRecipeComponentResourceCommand(newSelectedResourceUid, component.ResourceUid, setter);
        }
    }

    public class SetRecipeComponentSelectedRecipe : SetValueUndoableCommand<Guid?>
    {
        private SetRecipeComponentSelectedRecipe(Guid? newRecipe, Guid? previousRecipe, Func<Guid?, Task> setter)
            : base(newRecipe, previousRecipe, setter) { }

        public static async Task<SetRecipeComponentSelectedRecipe?> CreateAsync(IServiceProvider serviceProvider, Guid recipeComponentUid, Guid? newSelectedRecipeUid)
        {
            var recipeComponentService = serviceProvider.GetRequiredService<IRecipeComponentService>();
            var component = await recipeComponentService.GetComponentAsync(recipeComponentUid);
            if (component == null)
                throw new ArgumentException("Component not found with Uid: " + recipeComponentUid);

            var setter = CreateSetter<IRecipeComponentService, Guid?>(serviceProvider, recipeComponentUid, (s, uid, val) 
                => s.SetResourceSelectedRecipeAsync(uid, val));
            return new SetRecipeComponentSelectedRecipe(newSelectedRecipeUid, component.SelectedRecipeUid, setter);
        }
    }
}
