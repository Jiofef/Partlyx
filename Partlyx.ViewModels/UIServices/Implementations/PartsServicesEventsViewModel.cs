using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    // These events can be useful if subscribers need to know about a part only after it is added to the parent
    public record ResourceCreatingCompletedVMEvent(Guid ResourceUid) : IRoutedEvent { public object? ReceiverKey => ResourceUid; }
    public record RecipeCreatingCompletedVMEvent(Guid RecipeUid) : IRoutedEvent { public object? ReceiverKey => RecipeUid; }
    public record RecipeComponentCreatingCompletedVMEvent(Guid ComponentUid) : IRoutedEvent { public object? ReceiverKey => ComponentUid; }

    //public record ResourcesMovingCompletedVMEvent(Guid[] ResourceUids);  Resource moving commands were not implemented yet
    public record RecipesMovingCompletedVMEvent(Guid[] RecipeUids);
    public record RecipeComponentsMovingCompletedVMEvent(Guid[] ComponentUids);

    // -----

    public record ResourceUpdatedViewModelEvent(Guid ResourceUid, IReadOnlyList<string>? ChangedProperties, HashSet<object> ReceiverKeys) : IRoutedMultiKeyEvent;
    public record RecipeUpdatedViewModelEvent(Guid RecipeUid, IReadOnlyList<string>? ChangedProperties, HashSet<object> ReceiverKeys) : IRoutedMultiKeyEvent;
    public record RecipeComponentUpdatedViewModelEvent(Guid RecipeComponentUid, IReadOnlyList<string>? ChangedProperties, HashSet<object> ReceiverKeys) : IRoutedMultiKeyEvent;

    // -----

    public record ResourceDeletingStartedEvent(Guid ResourceUid) : IRoutedEvent { public object? ReceiverKey => ResourceUid; }
    public record RecipeDeletingStartedEvent(Guid RecipeUid, HashSet<object> ReceiverKeys) : IRoutedMultiKeyEvent;
    public record RecipeComponentDeletingStartedEvent(Guid ComponentUid, Guid ParentRecipeUid, HashSet<object> ReceiverKeys) : IRoutedMultiKeyEvent;
}
