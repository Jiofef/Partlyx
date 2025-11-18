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
    public class WorkingFileService : IWorkingFileService
    {
        private readonly IDBSaver _saver;
        private readonly IDBLoader _loader;
        private readonly IPartlyxRepository _repo;

        private readonly IEventBus _bus;

        public string? CurrentPartreePath { get; private set; }
        public bool IsChangesSaved { get; private set; } = true;

        // Will be needed if we need to return the IsChangesSaved when Undo until saved state
        private ICommand? _lastExcecutedCommand;

        public WorkingFileService(IDBSaver dbs, IDBLoader dbl, IPartlyxRepository repo, IEventBus bus)
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
            _bus.Publish(new FileChangesSavedUpdatedEvent(IsChangesSaved));
        }
        private void OnCommandUndoed(ICommand command, ICommand? previousCommand) 
        {
            _lastExcecutedCommand = command;
           
        }

        private void OnCurrentFileChanged()
        {
            IsChangesSaved = false;
            _bus.Publish(new FileChangesSavedUpdatedEvent(IsChangesSaved));
        }

        private void OnFilePathChanged(string? newFilePath)
        {
            _lastExcecutedCommand = null;

            IsChangesSaved = true;
            _bus.Publish(new FileChangesSavedUpdatedEvent(IsChangesSaved));

            CurrentPartreePath = newFilePath;
            var newFileName = Path.GetFileName(newFilePath);
            _bus.Publish(new CurrentFileNameChangedEvent(newFileName));
        }

        public async Task ClearCurrentFile()
        {
            await _repo.ClearEverything();

            IsChangesSaved = false;
            _bus.Publish(new FileChangesSavedUpdatedEvent(IsChangesSaved));

            OnFilePathChanged(null);
        }

        public async Task DeleteWorkingDB()
        {
            await _repo.DeleteWorkingDBFile();
        }

        public async Task<ExportResult> ExportPartreeAsync(string targetPath, CancellationToken cancellationToken = default)
        {
            var result = await _saver.ExportPartreeAsync(targetPath, cancellationToken);

            if (result.Success)
            {
                CurrentPartreePath = targetPath;
                _bus.Publish(new FileSavedEvent());


                _bus.Publish(new FileChangesSavedUpdatedEvent(IsChangesSaved));

                var newFileName = Path.GetFileName(targetPath);
                _bus.Publish(new CurrentFileNameChangedEvent(newFileName));
            }

            return result;
        }

        public async Task<ImportResult> ImportPartreeAsync(string partreePath, CancellationToken cancellationToken = default)
        {
            var result = await _loader.ImportPartreeAsync(partreePath, cancellationToken);

            if (result.Success)
                OnFilePathChanged(partreePath);

            return result;
        }
    }


    public record CurrentFileNameChangedEvent(string? newName);

    public record FileChangesSavedUpdatedEvent(bool IsChangesSaved);
}
