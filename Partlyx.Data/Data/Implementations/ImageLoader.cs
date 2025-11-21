using Partlyx.Infrastructure.Data.Interfaces;

namespace Partlyx.Infrastructure.Data.Implementations;

public class ImageLoader : IImageLoader
{
    public async Task<byte[]?> TryLoadImageAsync(string path)
    {
        if (!File.Exists(path))
            return null;

        byte[] imageData;
        await using (var fs = File.OpenRead(path))
        {
            imageData = new byte[fs.Length];
            await fs.ReadAsync(imageData, 0, imageData.Length);
        }

        return imageData;
    }
}
