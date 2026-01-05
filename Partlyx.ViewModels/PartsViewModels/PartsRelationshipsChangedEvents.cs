using Partlyx.Core.Partlyx;
using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.PartsViewModels
{
    public record RecipeResourceLinkChangedEvent(RecipeViewModel Recipe, Guid ResourceUid, RecipeResourceLinkTypeEnum LinkType);
}
