using Microsoft.EntityFrameworkCore;
using Partlyx.Core;
using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using System.Data;
using System.Data.Common;

namespace Partlyx.Infrastructure.Data.Implementations;

public class ImageDBRepository : IImagesRepository
{
    private readonly IDbContextFactory<PartlyxDBContext> _dbFactory;
    private readonly IEventBus _bus;
    public ImageDBRepository(IDbContextFactory<PartlyxDBContext> dbFactory, IEventBus bus)
    {
        _dbFactory = dbFactory;
        _bus = bus;
    }

    public async Task<Guid> AddImageAsync(PartlyxImage image)
    {
        using var ctx = await _dbFactory.CreateDbContextAsync();
        await ctx.Images.AddAsync(image);
        await ctx.SaveChangesAsync();

        _bus.Publish(new ImageAddedToDbEvent(image.Uid));
        return image.Uid;
    }
    public async Task DeleteImageAsync(Guid uid)
    {
        using var ctx = await _dbFactory.CreateDbContextAsync();
        var deletedRows = await ctx.Images
            .Where(u => u.Uid == uid)
            .ExecuteDeleteAsync();
        await ctx.SaveChangesAsync();

        if (deletedRows > 0)
            _bus.Publish(new ImageDeletedFromDbEvent(uid));
    }

    public async Task<bool> ExistsAsync(Guid uid)
    {
        using var ctx = await _dbFactory.CreateDbContextAsync();
        var result = await ctx.Images.AnyAsync(i => i.Uid == uid);
        return result;
    }

    public async Task<PartlyxImage?> GetImageAsync(Guid uid, bool includeCompressedContent = false, bool includeContent = false)
    {
        using var ctx = await _dbFactory.CreateDbContextAsync();
        var query = ctx.Images.AsQueryable();

        var result = await query
            .Where(i => i.Uid == uid)
            .Select(i => new PartlyxImage(uid)
            {
                Name = i.Name,
                Hash = i.Hash,
                Mime = i.Mime,
                Content = includeContent ? i.Content : Array.Empty<byte>(),
                CompressedContent = includeCompressedContent ? i.CompressedContent : Array.Empty<byte>(),
            })
            .FirstOrDefaultAsync();

        return result;
    }
    public async Task<List<PartlyxImage>> GetAllTheImagesAsync(bool includeCompressedContent = false, bool includeContent = false)
    {
        using var ctx = await _dbFactory.CreateDbContextAsync();
        var query = ctx.Images.AsQueryable();

        var result = await query
            .Select(i => new PartlyxImage(i.Uid)
            {
                Name = i.Name,
                Hash = i.Hash,
                Mime = i.Mime,
                Content = includeContent ? i.Content : Array.Empty<byte>(),
                CompressedContent = includeCompressedContent ? i.CompressedContent : Array.Empty<byte>(),
            })
            .ToListAsync();

        return result;
    }

    public async Task<Stream> GetImageStreamAsync(Guid uid)
    {
        using var ctx = await _dbFactory.CreateDbContextAsync();
        var streamProv = new StreamImageProvider(ctx);
        var stream = await streamProv.OpenStreamAsync(uid);
        return stream;
    }

    public async Task<(bool exists, Guid? imageUid)> ImageWithSameHashExistsAsync(byte[] imageHash)
    {
        using var ctx = await _dbFactory.CreateDbContextAsync();
        var imageUid = await ctx.Images
            .Where(i => i.Hash == imageHash)
            .Select(i => i.Uid)
            .FirstOrDefaultAsync();
        var result = (imageUid != Guid.Empty);
        return (result, imageUid);
    }

    public async Task<TResult> ExecuteOnImageAsync<TResult>(Guid imageUid, Func<PartlyxImage, Task<TResult>> action)
    {
        await using var db = _dbFactory.CreateDbContext();

        var i = await db.Images
            .FirstOrDefaultAsync(x => x.Uid == imageUid);

        if (i == null) throw new KeyNotFoundException();

        var result = await action(i);

        await db.SaveChangesAsync();

        return result;
    }

    public Task ExecuteOnImageAsync(Guid imageUid, Func<PartlyxImage, Task> action)
    {
        return ExecuteOnImageAsync(imageUid, async i =>
        {
            await action(i);
            return true; // Dummy value
        });
    }
}

public record ImageAddedToDbEvent(Guid ImageUid);

public record ImageDeletedFromDbEvent(Guid ImageUid);