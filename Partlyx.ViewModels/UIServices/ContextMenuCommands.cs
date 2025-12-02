namespace Partlyx.ViewModels.UIServices
{
    public abstract class ContextMenuCommands : PartlyxObservable
    {
        public abstract void AllowAll();
        public abstract void UpdateAllowed();
    }
}