using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Core.Contracts;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public partial class MainWindowController : ObservableObject, IMainWindowController
    {
        public MainWindowNameController NameController { get; }
        public MainWindowController(ILocalizationService localization, IEventBus bus)
        {
            NameController = new MainWindowNameController(this, localization, bus);

            _windowTitle = NameController.DefaultName;
        }

        private string _windowTitle;
        public string WindowTitle { get => _windowTitle; set => SetProperty(ref _windowTitle, value); }
    }
}
