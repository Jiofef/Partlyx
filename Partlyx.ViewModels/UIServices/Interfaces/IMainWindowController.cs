using Partlyx.ViewModels.UIServices.Implementations;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IMainWindowController
    {
        string WindowTitle { get; set; }
        MainWindowNameController NameController { get; }
    }
}