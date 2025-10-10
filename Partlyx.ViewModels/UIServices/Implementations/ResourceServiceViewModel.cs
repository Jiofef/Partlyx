using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public partial class ResourceServiceViewModel
    {
        private readonly ICommandServices _commands;
        private readonly IGlobalSelectedParts _selectedParts;

        public ResourceServiceViewModel(ICommandServices cs, IGlobalSelectedParts gsp) 
        {
            _commands = cs;
            _selectedParts = gsp;
        }

        [RelayCommand]
        public async Task CreateResourceAsync()
        {
            // It must be executed on a single thread so that recipients respond to events immediately after they are sent
            await Task.Run(async () =>
            {
                await _commands.Dispatcher.ExcecuteComplexAsync(async complexDispatcher =>
                {
                    // Resource creating
                    var createResourceCommand = _commands.Factory.Create<CreateResourceCommand>();
                    await complexDispatcher.ExcecuteAsync(createResourceCommand);
                    var resourseUid = createResourceCommand.ResourceUid;

                    // Default recipe creating
                    var defaultRecipeCreateCommand = _commands.Factory.Create<CreateRecipeCommand>(resourseUid);
                    await complexDispatcher.ExcecuteAsync(defaultRecipeCreateCommand);

                    var recipeUid = defaultRecipeCreateCommand.RecipeUid;
                    var recipeName = "Default recipe";
                    var defaultRecipeSetNameCommand = await _commands.Factory.CreateAsync<SetRecipeNameCommand>(resourseUid, recipeUid, recipeName);
                    await complexDispatcher.ExcecuteAsync(defaultRecipeSetNameCommand);
                });
            });
        }

        [RelayCommand]
        public void StartRenamingSelected()
        {
            var resourceVM = _selectedParts.GetSingleResourceOrNull();
            if (resourceVM == null) return;

            resourceVM.Ui.IsRenaming = true;
        }

        [RelayCommand]
        public async Task RemoveAsync(ResourceItemViewModel resource)
        {
            await _commands.CreateSyncAndExcecuteAsync<DeleteResourceCommand>(resource.Uid);
        }
    }
}
