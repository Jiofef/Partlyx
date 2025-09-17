using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Infrastructure.Data.CommonFileEvents
{
    public record PartlyxDBInitializedEvent(string path);

    public record SaveStartedEvent(string destination, Guid correlationUid);

    public record SaveCompletedEvent(string destination, Guid correlationUid);

    public record SaveFailedEvent(string destination, Guid correlationUid);
}
