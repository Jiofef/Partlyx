using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core;
using Partlyx.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services.Commands.RecipeComponentCommonCommands
{
    public class CreateRecipeComponentCommand : IUndoableCommand
    {
        private IRecipeComponentService _recipeComponentService;

        private Guid _resourceUid;
        private Guid _recipeUid;
        private Guid _componentResourceUid;

        private Guid _recipeComponentUid;

        public CreateRecipeComponentCommand(IServiceProvider serviceProvider, Guid grandParentResourceUid, Guid parentRecipeUid, Guid componentResourceUid)
        {
            _recipeComponentService = serviceProvider.GetRequiredService<IRecipeComponentService>();
            _resourceUid = grandParentResourceUid;
            _recipeUid = parentRecipeUid;
            _componentResourceUid = componentResourceUid;
        }

        public async Task ExecuteAsync()
        {
            Guid uid = await _recipeComponentService.CreateComponentAsync(_resourceUid, _recipeUid, _componentResourceUid);
            _recipeComponentUid = uid;
        }

        public async Task UndoAsync()
        {
            await _recipeComponentService.DeleteComponentAsync(_resourceUid, _recipeComponentUid);
            _recipeComponentUid = Guid.Empty;
        }
    }

    public class DeleteRecipeComponentCommand : IUndoableCommand
    {
        private IRecipeComponentService _recipeComponentService;
        private IResourceRepository _resourceRepository;

        private Guid _resourceUid;
        private Guid _recipeUid;
        private Guid _recipeComponentUid;

        private RecipeComponent? _deletedRecipeComponent;

        public DeleteRecipeComponentCommand(IServiceProvider serviceProvider, Guid grandParentResourceUid, Guid parentRecipeUid, Guid recipeComponentUid)
        {
            _recipeComponentService = serviceProvider.GetRequiredService<IRecipeComponentService>();
            _resourceRepository = serviceProvider.GetRequiredService<IResourceRepository>();

            _resourceUid = grandParentResourceUid;
            _recipeUid = parentRecipeUid;
            _recipeComponentUid = recipeComponentUid;
        }

        public async Task ExecuteAsync()
        {
            var resource = await _resourceRepository.GetByUidAsync(_resourceUid);

            if (resource == null)
                throw new ArgumentNullException(nameof(resource));

            var recipe = resource.GetRecipeByUid(_recipeUid);

            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe));

            _deletedRecipeComponent = recipe.GetRecipeComponentByUid(_recipeComponentUid);

            await _recipeComponentService.DeleteComponentAsync(_resourceUid, _recipeComponentUid);
        }

        public async Task UndoAsync()
        {
            if (_deletedRecipeComponent == null) return;

            await _resourceRepository.ExecuteOnRecipeAsync(_resourceUid, _recipeUid,
                (recipe) =>
                {
                    _deletedRecipeComponent.AttachTo(recipe);
                    return Task.CompletedTask;
                });

            _deletedRecipeComponent = null;
        }
    }

    public class DuplicateRecipeComponentCommand : IUndoableCommand
    {
        private IRecipeComponentService _recipeComponentService;
        private Guid _resourceUid;
        private Guid _recipeComponentUid;
        private Guid _duplicateUid;

        public DuplicateRecipeComponentCommand(IServiceProvider serviceProvider, Guid grandParentResourceUid, Guid recipeComponentUid)
        {
            _recipeComponentService = serviceProvider.GetRequiredService<IRecipeComponentService>();
            _resourceUid = grandParentResourceUid;
            _recipeComponentUid = recipeComponentUid;
        }

        public async Task ExecuteAsync()
        {
            _duplicateUid = await _recipeComponentService.DuplicateComponentAsync(_resourceUid, _recipeComponentUid);
        }

        public async Task UndoAsync()
        {
            await _recipeComponentService.DeleteComponentAsync(_resourceUid, _duplicateUid);
            _duplicateUid = Guid.Empty;
        }
    }

    public class SetRecipeComponentQuantityCommand : SetValueUndoableCommand<double>
    {
        private SetRecipeComponentQuantityCommand(double quantity, double savedValue, Func<double, Task> setter)
            : base(quantity, savedValue, setter) { }

        public static async Task<SetRecipeComponentQuantityCommand?> CreateAsync(IServiceProvider serviceProvider, Guid grandParentResourceUid, Guid recipeComponentUid, double amount)
        {
            var resourceService = serviceProvider.GetRequiredService<IRecipeComponentService>();
            var component = await resourceService.GetComponentAsync(grandParentResourceUid, recipeComponentUid);
            if (component == null)
                throw new ArgumentException("Component not found with Uid: " + recipeComponentUid);

            return new SetRecipeComponentQuantityCommand(amount, component.Quantity, async (value) =>
            {
                await resourceService.SetQuantityAsync(grandParentResourceUid, recipeComponentUid, value);
            });
        }
    }

    public class SetRecipeComponentResourceCommand : SetValueUndoableCommand<Guid>
    {
        private SetRecipeComponentResourceCommand(Guid newResource, Guid previousResource, Func<Guid, Task> setter)
            : base(newResource, previousResource, setter) { }

        public static async Task<SetRecipeComponentResourceCommand?> CreateAsync(IServiceProvider serviceProvider, Guid grandParentResourceUid, Guid recipeComponentUid, Guid newSelectedResourceUid)
        {
            var recipeComponentService = serviceProvider.GetRequiredService<IRecipeComponentService>();
            var component = await recipeComponentService.GetComponentAsync(grandParentResourceUid, recipeComponentUid);
            if (component == null)
                throw new ArgumentException("Component not found with Uid: " + recipeComponentUid);

            return new SetRecipeComponentResourceCommand(newSelectedResourceUid, component.ResourceUid, async (value) =>
            {
                await recipeComponentService.SetComponentResourceAsync(grandParentResourceUid, recipeComponentUid, value);
            });
        }
    }
}
