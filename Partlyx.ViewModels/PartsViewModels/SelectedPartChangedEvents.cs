using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels
{
    // Full information
    public record GlobalSelectedResourcesChangedEvent(Guid[] newSelected);

    public record GlobalSelectedRecipesChangedEvent(Guid[] newSelected);

    public record GlobalSelectedComponentsChangedEvent(Guid[] newSelected);

    // Multi-selection
    public record GlobalResourceAddedToSelectedEvent(Guid newSelected);

    public record GlobalRecipeAddedToSelectedEvent(Guid newSelected);

    public record GlobalComponentAddedToSelectedEvent(Guid newSelected);

    // Single selection
    public record GlobalSingleResourceSelectedEvent(Guid selected);

    public record GlobalSingleRecipeSelectedEvent(Guid selected);

    public record GlobalSingleComponentSelectedEvent(Guid selected);
}
