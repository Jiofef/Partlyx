using Partlyx.Services.Dtos;

namespace Partlyx.Services
{
    public record ImagesDBInitializationStartedEvent();
    public record ImagesDBInitializationFinishedEvent();
    public record ImagesBulkLoadedEvent(ImageDto[] Images);

    public record ImageUpdatedEvent(ImageDto Image, IReadOnlyList<string>? ChangedProperties);
}
