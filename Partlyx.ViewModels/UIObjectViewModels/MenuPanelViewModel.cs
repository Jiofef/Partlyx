using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public class MenuPanelViewModel
    {
        public MenuPanelFileViewModel FileMenu { get; }
        public MenuPanelEditViewModel EditMenu { get; }
        public MenuPanelSettingsViewModel SettingsMenu { get; }
        public MenuPanelProjectViewModel ProjectMenu { get; }
        public MenuPanelHelpViewModel HelpMenu { get; }

        public MenuPanelViewModel(MenuPanelFileViewModel fileMenu, MenuPanelEditViewModel editMenu, MenuPanelSettingsViewModel settingsMenu, MenuPanelProjectViewModel projectMenu,
            MenuPanelHelpViewModel helpMenu)
        {
            FileMenu = fileMenu;
            EditMenu = editMenu;
            SettingsMenu = settingsMenu;
            ProjectMenu = projectMenu;
            HelpMenu = helpMenu;
        }
    }
}