using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    // These events can be useful if subscribers need to know about a part only after it is added to the parent
    public record ResourceCreatingCompletedVMEvent(Guid ResourceUid);
    public record RecipeCreatingCompletedVMEvent(Guid RecipeUid);
    public record RecipeComponentCreatingCompletedVMEvent(Guid ComponentUid);

    //public record ResourcesMovingCompletedVMEvent(Guid[] ResourceUids);  Resource moving commands were not implemented yet
    public record RecipesMovingCompletedVMEvent(Guid[] RecipeUids);
    public record RecipeComponentsMovingCompletedVMEvent(Guid[] ComponentUids);
}
