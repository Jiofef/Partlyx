using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public record ResourceVMAddedToStoreEvent(Guid resourceUid);
    public record ResourceVMRemovedFromStoreEvent(Guid resourceUid);

    public record RecipeVMAddedToStoreEvent(Guid recipeUid);
    public record RecipeVMRemovedFromStoreEvent(Guid recipeUid);

    public record RecipeComponentVMAddedToStoreEvent(Guid componentUid);
    public record RecipeComponentVMRemovedFromStoreEvent(Guid componentUid);
}
