using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core.VisualsInfo;
using Partlyx.Services.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Partlyx.Services.Commands.AbstractIconCommands;

namespace Partlyx.Services.Commands.ResourceIconCommands
{
    // Figure commands
    public class SetRecipeIconFigureTypeCommand : SetIconFigureTypeCommand
    {
        private SetRecipeIconFigureTypeCommand(string type, string savedType, Func<string, Task> setter)
            : base(type, savedType, setter) { }

        public static async Task<SetRecipeIconFigureTypeCommand?> CreateAsync(IServiceProvider serviceProvider, Guid parentResourceUid, Guid recipeUid, string type)
        {
            var iconService = serviceProvider.GetRequiredService<IRecipeFigureIconService>();
            return await SetIconFigureTypeCommand.CreateAsync(serviceProvider, iconService, type, parentResourceUid, recipeUid) as SetRecipeIconFigureTypeCommand;
        }
    }

    public class SetRecipeIconFigureColorCommand : SetIconFigureColorCommand
    {
        private SetRecipeIconFigureColorCommand(Color color, Color savedcolor, Func<Color, Task> setter)
            : base(color, savedcolor, setter) { }

        public static async Task<SetRecipeIconFigureColorCommand?> CreateAsync(IServiceProvider serviceProvider, Guid parentResourceUid, Guid recipeUid, Color color)
        {
            var iconService = serviceProvider.GetRequiredService<IRecipeFigureIconService>();
            return await SetIconFigureColorCommand.CreateAsync(serviceProvider, iconService, color, parentResourceUid, recipeUid) as SetRecipeIconFigureColorCommand;
        }
    }

    // Image commands
    public class SetRecipeIconImageUidCommand : SetIconImageUidCommand
    {
        private SetRecipeIconImageUidCommand(Guid uid, Guid savedUid, Func<Guid, Task> setter)
            : base(uid, savedUid, setter) { }

        public static async Task<SetRecipeIconImageUidCommand?> CreateAsync(IServiceProvider serviceProvider, Guid parentResourceUid, Guid recipeUid, Guid uid)
        {
            var iconService = serviceProvider.GetRequiredService<IRecipeImageIconService>();
            return await SetIconImageUidCommand.CreateAsync(serviceProvider, iconService, uid, parentResourceUid, recipeUid) as SetRecipeIconImageUidCommand;
        }
    }
}
