using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class MenuPanelViewModel
    {
        public MenuPanelFileViewModel FileMenu { get; }
        public MenuPanelEditViewModel EditMenu { get; }

        public MenuPanelViewModel(MenuPanelFileViewModel mpfvm, MenuPanelEditViewModel editMenu)
        {
            FileMenu = mpfvm;
            EditMenu = editMenu;
        }
    }
}
