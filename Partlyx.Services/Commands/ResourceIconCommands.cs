using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core.VisualsInfo;
using Partlyx.Services.Commands.AbstractIconCommands;
using Partlyx.Services.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services.Commands.ResourceIconCommands
{
    // Figure commands
    public class SetResourceIconFigureTypeCommand : SetIconFigureTypeCommand
    {
        private SetResourceIconFigureTypeCommand(string type, string savedType, Func<string, Task> setter)
            : base(type, savedType, setter) { }

        public static async Task<SetResourceIconFigureTypeCommand?> CreateAsync(IServiceProvider serviceProvider, Guid parentResourceUid, string type)
        {
            var iconService = serviceProvider.GetRequiredService<IResourceFigureIconService>();
            return await SetIconFigureTypeCommand.CreateAsync(serviceProvider, iconService, type, parentResourceUid) as SetResourceIconFigureTypeCommand;
        }
    }

    public class SetResourceIconFigureColorCommand : SetIconFigureColorCommand
    {
        private SetResourceIconFigureColorCommand(Color color, Color savedcolor, Func<Color, Task> setter)
            : base(color, savedcolor, setter) { }

        public static async Task<SetResourceIconFigureColorCommand?> CreateAsync(IServiceProvider serviceProvider, Guid parentResourceUid, Color color)
        {
            var iconService = serviceProvider.GetRequiredService<IResourceFigureIconService>();
            return await SetIconFigureColorCommand.CreateAsync(serviceProvider, iconService, color, parentResourceUid) as SetResourceIconFigureColorCommand;
        }
    }

    // Image commands
    public class SetResourceIconImageUidCommand : SetIconImageUidCommand
    {
        private SetResourceIconImageUidCommand(Guid uid, Guid savedUid, Func<Guid, Task> setter)
            : base(uid, savedUid, setter) { }

        public static async Task<SetResourceIconImageUidCommand?> CreateAsync(IServiceProvider serviceProvider, Guid parentResourceUid, Guid uid)
        {
            var iconService = serviceProvider.GetRequiredService<IResourceImageIconService>();
            return await SetIconImageUidCommand.CreateAsync(serviceProvider, iconService, uid, parentResourceUid) as SetResourceIconImageUidCommand;
        }
    }
}
