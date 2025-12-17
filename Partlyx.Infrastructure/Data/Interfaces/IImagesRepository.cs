using Partlyx.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Infrastructure.Data.Interfaces
{
    public interface IImagesRepository
    {
        Task<Guid> AddImageAsync(PartlyxImage image);
        Task DeleteImageAsync(Guid uid);
        Task<TResult> ExecuteOnImageAsync<TResult>(Guid imageUid, Func<PartlyxImage, Task<TResult>> action);
        Task ExecuteOnImageAsync(Guid imageUid, Func<PartlyxImage, Task> action);
        Task<bool> ExistsAsync(Guid uid);
        Task<List<PartlyxImage>> GetAllTheImagesAsync(bool includeCompressedContent = false, bool includeContent = false);
        Task<PartlyxImage?> GetImageAsync(Guid uid, bool includeCompressedContent = false, bool includeContent = false);
        Task<Stream> GetImageStreamAsync(Guid uid);
        Task<(bool exists, Guid? imageUid)> ImageWithSameHashExistsAsync(byte[] imageHash);
    }
}
