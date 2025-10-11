using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.OtherEvents;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.Infrastructure.Data.Implementations
{
    /// <summary>
    /// Facade for IDBSaver, IDBLoader and IResourceRepository
    /// </summary>
    public class FileService : IFileService
    {
        private readonly IDBSaver _saver;
        private readonly IDBLoader _loader;
        private readonly IPartsRepository _repo;

        private readonly IEventBus _bus;

        public string? CurrentPartreePath { get; private set; }
        public bool IsChangesSaved { get; private set; } = true;

        // Will be needed if we need to return the IsChangesSaved when Undo until saved
        private ICommand? _lastExcecutedCommand;

        public FileService(IDBSaver dbs, IDBLoader dbl, IPartsRepository repo, IEventBus bus)
        {
            _saver = dbs;
            _loader = dbl;
            _repo = repo;
            _bus = bus;

            bus.Subscribe<CommandExcecutedEvent>(ev => OnCommandExcecuted(ev.Command), true);
            bus.Subscribe<CommandRedoedEvent>(ev => OnCommandExcecuted(ev.Command), true);
            bus.Subscribe<CommandUndoedEvent>(ev => OnCommandUndoed(ev.Command, ev.PreviousCommand), true);
        }

        private void OnCommandExcecuted(ICommand command)
        {
            _lastExcecutedCommand = command;
            IsChangesSaved = false;
        }
        private void OnCommandUndoed(ICommand command, ICommand? previousCommand) 
        {
            _lastExcecutedCommand = command;
            IsChangesSaved = false;
        }

        private void OnSelectedFileChanged(string? newFilePath)
        {
            _lastExcecutedCommand = null;
            IsChangesSaved = true;
        }

        public async Task ClearCurrentFile()
        {
            await _repo.ClearEverything();
            IsChangesSaved = false;

            OnSelectedFileChanged(null);
        }

        public async Task<ExportResult> ExportPartreeAsync(string targetPath, CancellationToken cancellationToken = default)
        {
            var result = await _saver.ExportPartreeAsync(targetPath, cancellationToken);

            if (result.Success)
            {
                CurrentPartreePath = targetPath;
                _bus.Publish(new FileSavedEvent());
            }

            return result;
        }

        public async Task<ImportResult> ImportPartreeAsync(string partreePath, CancellationToken cancellationToken = default)
        {
            var result = await _loader.ImportPartreeAsync(partreePath, cancellationToken);

            if (result.Success)
                OnSelectedFileChanged(partreePath);

            return result;
        }
    }
}
