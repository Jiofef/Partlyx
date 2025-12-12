using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;

namespace Partlyx.Services.PartsEventClasses
{
    // Resource events
    public record ResourceUpdatedEvent(ResourceDto Resource, IReadOnlyList<string>? ChangedProperties, object? ReceiverKey) : IRoutedEvent;

    public record ResourceDeletedEvent(Guid ResourceUid, object? ReceiverKey) : IRoutedEvent;

    public record ResourceCreatedEvent(ResourceDto Resource, object? ReceiverKey) : IRoutedEvent;

    // Recipe events
    public record RecipeUpdatedEvent(RecipeDto Recipe, IReadOnlyList<string>? ChangedProperties, object? ReceiverKey) : IRoutedEvent;

    public record RecipeDeletedEvent(Guid ParentResourceUid, Guid RecipeUid, object? ReceiverKey) : IRoutedEvent;

    public record RecipeCreatedEvent(RecipeDto Recipe, object? ReceiverKey) : IRoutedEvent;

    public record RecipeMovedEvent(Guid OldResourceUid, Guid NewResourceUid, Guid RecipeUid, HashSet<object> ReceiverKeys) : IRoutedMultiKeyEvent;

    // Recipe component events
    public record RecipeComponentUpdatedEvent(RecipeComponentDto RecipeComponent, IReadOnlyList<string>? ChangedProperties, object? ReceiverKey) : IRoutedEvent;

    public record RecipeComponentDeletedEvent(Guid GrandParentResourceUid, Guid ParentRecipeUid, Guid RecipeComponentUid, object? ReceiverKey) : IRoutedEvent;

    public record RecipeComponentCreatedEvent(RecipeComponentDto RecipeComponent, object? ReceiverKey) : IRoutedEvent;

    public record RecipeComponentMovedEvent(Guid OldResourceUid, Guid NewResourceUid, Guid OldRecipeUid, Guid NewRecipeUid, Guid ComponentUid, HashSet<object> ReceiverKeys) : IRoutedMultiKeyEvent;

    // Parts initialization events
    public record PartsInitializationStartedEvent();

    public record ResourcesBulkLoadedEvent(ResourceDto[] Bulk);

    public record RecipesBulkLoadedEvent(RecipeDto[] Bulk);

    public record RecipeComponentsBulkLoadedEvent(RecipeComponentDto[] Bulk);

    public record PartsInitializationFinishedEvent();
}
