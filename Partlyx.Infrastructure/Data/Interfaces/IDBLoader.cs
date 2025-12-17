using Partlyx.Infrastructure.Data.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Infrastructure.Data.Interfaces
{
    public interface IDBLoader
    {
        Task<ImportResult> ImportPartreeAsync(string partreePath, CancellationToken cancellationToken = default);
    }
}
