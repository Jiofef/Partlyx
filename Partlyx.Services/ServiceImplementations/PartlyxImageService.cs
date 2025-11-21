using Partlyx.Core;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.Helpers;
using Partlyx.Services.ServiceInterfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services.ServiceImplementations
{
    public class PartlyxImageService : IPartlyxImageService
    {
        public readonly int MiniatureWidth = 128;
        public readonly int MiniatureHeight = 128;
        private readonly IImagesRepository _repo;
        private readonly IImageLoader _imageLoader;
        private readonly IEventBus _bus;
        public PartlyxImageService(IImagesRepository repo, IImageLoader loader, IEventBus bus)
        {
            _repo = repo;
            _imageLoader = loader;
            _bus = bus;
        }

        public async Task<ImageDto?> GetImageOrNullAsync(Guid uid)
        {
            var imageNoContent = await _repo.GetImageAsync(uid, true);
            var contentStream = await _repo.GetImageStreamAsync(uid);

            if (imageNoContent == null || contentStream == null) return null;
            var content = await StreamToBytesAsync(contentStream);

            var dto = new ImageDto(imageNoContent.Uid, imageNoContent.Name, imageNoContent.Mime, imageNoContent.Hash, content, imageNoContent.CompressedContent);
            return dto;
        }
        public async Task<byte[]?> GetFullImageOrNullAsync(Guid uid)
        {
            var contentStream = await _repo.GetImageStreamAsync(uid);

            if (contentStream == null) return null;
            var content = await StreamToBytesAsync(contentStream);
            return content;
        }
        private async Task<byte[]> StreamToBytesAsync(Stream stream)
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();
            return bytes;
        }
        public async Task SetImageNameAsync(Guid uid, string name)
        {
            await _repo.ExecuteOnImageAsync(uid, (image) =>
            {
                image.Name = name;
                return Task.CompletedTask;
            });
            var image = await GetImageOrNullAsync(uid);
            if (image == null) return;

            _bus.Publish(new ImageUpdatedEvent(image, ["Name"]));
        }
        public async Task DeleteImageAsync(Guid uid)
        {
            await _repo.DeleteImageAsync(uid);

            _bus.Publish(new ImageDeletedFromDbEvent(uid));
        }

        public async Task<byte[]?> TryLoadFromDiskAsync(string path)
        {
            var imageBytes = await _imageLoader.TryLoadImageAsync(path);
            return imageBytes;
        }

        public async Task<Guid?> TryLoadFromDiskToDbAsync(string path)
        {
            var imageBytes = await TryLoadFromDiskAsync(path);

            if (imageBytes == null)
                return null;

            var hash = ByteHelper.ComputeHash(imageBytes);
            var sameImageExistsResult = await _repo.ImageWithSameHashExistsAsync(hash);
            if (sameImageExistsResult.exists)
                return sameImageExistsResult.imageUid;

            string mime = GetMimeType(path);

            if (mime == "not_supported")
                return null;

            if (mime == "image_webp")
                imageBytes = ImageUtils.ConvertWebpToPng(imageBytes);

            // resizing
            var compressedImageBytes = ImageUtils.Resize(imageBytes, MiniatureWidth, MiniatureHeight, mime);

            var image = new PartlyxImage()
            {
                Name = Path.GetFileName(path),
                Hash = ByteHelper.ComputeHash(imageBytes),
                Content = imageBytes,
                CompressedContent = compressedImageBytes,
                Mime = mime
            };

            await _repo.AddImageAsync(image);
            return image.Uid;
        }
        public bool IsFileSupported(string path)
        {
            string mime = GetMimeType(path);
            return mime != "not_supported";
        }
        private static string GetMimeType(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();
            return ext switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                _ => "not_supported"
            };
        }
    }
}
