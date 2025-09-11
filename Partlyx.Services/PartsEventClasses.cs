using Partlyx.Services.Dtos;

namespace Partlyx.Services.PartsEventClasses
{
    // Resource events
    public record ResourceUpdatedEvent(ResourceDto Resource, IReadOnlyList<string>? ChangedProperties);

    public record ResourceDeletedEvent(Guid ResourceUid);

    public record ResourceCreatedEvent(ResourceDto Resource);

    // Recipe events
    public record RecipeUpdatedEvent(RecipeDto Recipe, IReadOnlyList<string>? ChangedProperties);

    public record RecipeDeletedEvent(Guid ParentResourceUid, Guid RecipeUid);

    public record RecipeCreatedEvent(RecipeDto Recipe);

    public record RecipeMovedEvent(Guid OldResourceUid, Guid NewResourceUid, Guid RecipeUid);

    // Recipe component events
    public record RecipeComponentUpdatedEvent(RecipeComponentDto RecipeComponent, IReadOnlyList<string>? ChangedProperties);

    public record RecipeComponentDeletedEvent(Guid GrandParentResourceUid, Guid ParentRecipeUid, Guid RecipeComponentUid);

    public record RecipeComponentCreatedEvent(RecipeComponentDto RecipeComponent);

    public record RecipeComponentMovedEvent(Guid OldResourceUid, Guid OldRecipeUid, Guid NewResourceUid, Guid NewRecipeUid, Guid RecipeComponentUid);

    // Bulk loaded events
    public record ResourcesBulkLoadedEvent(ResourceDto[] Bulk);

    public record RecipesBulkLoadedEvent(RecipeDto[] Bulk);

    public record RecipeComponentsBulkLoadedEvent(RecipeComponentDto[] Bulk);
}
