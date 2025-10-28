using Partlyx.Services.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Partlyx.Services.Commands.AbstractIconCommands
{
    public class SetIconFigureTypeCommand : SetValueUndoableCommand<string>
    {
        protected SetIconFigureTypeCommand(string type, string savedType, Func<string, Task> setter)
            : base(type, savedType, setter) { }

        public static async Task<SetIconFigureTypeCommand?> CreateAsync(IServiceProvider serviceProvider, IFigureIconService service, string type, params Guid[] parentResourcesUid)
        {
            var icon = await service.GetFigureIconAsync();

            if (icon == null)
                throw new InvalidOperationException("Figure icon was not found");

            return new SetIconFigureTypeCommand(type, icon.FigureType, async (value) =>
            {
                await service.SetFigureTypeAsync(value, parentResourcesUid);
            });
        }
    }


    public class SetIconFigureColorCommand : SetValueUndoableCommand<Color>
    {
        protected SetIconFigureColorCommand(Color color, Color savedcolor, Func<Color, Task> setter)
            : base(color, savedcolor, setter) { }

        public static async Task<SetIconFigureColorCommand?> CreateAsync(IServiceProvider serviceProvider, IFigureIconService service, Color color, params Guid[] parentResourcesUid)
        {
            var icon = await service.GetFigureIconAsync();

            if (icon == null)
                throw new InvalidOperationException("Figure icon was not found");

            return new SetIconFigureColorCommand(color, icon.Color, async (value) =>
            {
                await service.SetFigureColorAsync(color, parentResourcesUid);
            });
        }
    }

    // Image commands
    public class SetIconImageUidCommand : SetValueUndoableCommand<Guid>
    {
        protected SetIconImageUidCommand(Guid uid, Guid savedUid, Func<Guid, Task> setter)
            : base(uid, savedUid, setter) { }

        public static async Task<SetIconImageUidCommand?> CreateAsync(IServiceProvider serviceProvider, IImageIconService service, Guid uid, params Guid[] parentResourcesUid)
        {
            var icon = await service.GetImageIconAsync();

            if (icon == null)
                throw new InvalidOperationException("Image icon was not found");

            return new SetIconImageUidCommand(uid, icon.Uid, async (value) =>
            {
                await service.SetImageUidAsync(uid, parentResourcesUid);
            });
        }
    }
}