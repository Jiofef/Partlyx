using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Services.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Infrastructure.Data.Implementations
{
    /// <summary>
    /// Facade for IDBSaver, IDBLoader and IResourceRepository
    /// </summary>
    public class FileService : IFileService
    {
        private readonly IDBSaver _saver;
        private readonly IDBLoader _loader;
        private readonly IResourceRepository _repo;

        public string? CurrentPartreePath { get; private set; }

        public FileService(IDBSaver dbs, IDBLoader dbl, IResourceRepository repo)
        {
            _saver = dbs;
            _loader = dbl;
            _repo = repo;
        }

        public async Task ClearCurrentFile()
        {
            await _repo.ClearEverything();
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
