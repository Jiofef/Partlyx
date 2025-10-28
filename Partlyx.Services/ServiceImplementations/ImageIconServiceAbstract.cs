using Partlyx.Core.VisualsInfo;

namespace Partlyx.Services.ServiceImplementations
{
    public abstract class ImageIconServiceAbstract
    {
        public async Task SetImageUidAsync(Guid uid, params Guid[] parentUids)
        {
            await TryExcecuteOnImageIconAsync(icon =>
            {
                icon.Uid = uid;
                return Task.CompletedTask;
            },
            parentUids);
        }

        public async Task<ImageIcon?> GetImageIconAsync(params Guid[] parentUids)
        {
            ImageIcon? result = null;
            await TryExcecuteOnImageIconAsync(icon =>
            {
                result = icon;
                return Task.CompletedTask;
            },
            parentUids);
            return result;
        }

        /// <summary>
        /// Should automate getting icon object and saving changes to changed entity and DB.
        /// </summary>
        protected abstract Task TryExcecuteOnImageIconAsync(Func<ImageIcon, Task> action, params Guid[] parentUids);
    }
}
