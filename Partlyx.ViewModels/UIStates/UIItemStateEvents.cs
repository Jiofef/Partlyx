using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIStates
{
    public record SetAllThePartItemsExpandedEvent(bool expand);

    public record SetAllTheResourceItemsExpandedEvent(bool expand);
    public record SetAllTheRecipeItemsExpandedEvent(bool expand);
    public record SetAllTheRecipeComponentItemsExpandedEvent(bool expand);
}
