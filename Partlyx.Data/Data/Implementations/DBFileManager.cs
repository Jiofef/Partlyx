using Partlyx.Infrastructure.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Infrastructure.Data.Implementations
{
    /// <summary>
    /// Facade for IDBSaver and IDBLoader
    /// </summary>
    public class DBFileManager : IDBFileManager
    {
        private readonly IDBSaver _saver;
        private readonly IDBLoader _loader;

        public string? CurrentPartreePath { get; private set; }

        public DBFileManager(IDBSaver dbs, IDBLoader dbl)
        {
            _saver = dbs;
            _loader = dbl;
        }

        public async Task<ExportResult> ExportPartreeAsync(string targetPath, CancellationToken cancellationToken = default)
        {
            var result = await _saver.ExportPartreeAsync(targetPath, cancellationToken);

            if (result.Success)
                CurrentPartreePath = targetPath;

            return result;
        }

        public async Task<ImportResult> ImportPartreeAsync(string partreePath, CancellationToken cancellationToken = default)
        {
            var result = await _loader.ImportPartreeAsync(partreePath, cancellationToken);

            if (result.Success)
                CurrentPartreePath = partreePath;

            return result;
        }
    }
}
