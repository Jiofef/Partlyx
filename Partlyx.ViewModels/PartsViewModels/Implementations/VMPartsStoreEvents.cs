using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public record ResourceVMAddedToStoreEvent(Guid ResourceUid);
    public record ResourceVMRemovedFromStoreEvent(Guid ResourceUid);

    public record RecipeVMAddedToStoreEvent(Guid RecipeUid);
    public record RecipeVMRemovedFromStoreEvent(Guid RecipeUid);

    public record RecipeComponentVMAddedToStoreEvent(Guid ComponentUid);
    public record RecipeComponentVMRemovedFromStoreEvent(Guid ComponentUid);
}
