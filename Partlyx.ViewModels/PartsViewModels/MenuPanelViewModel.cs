using Partlyx.ViewModels.UIObjectViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels
{
    public partial class MenuPanelViewModel
    {
        public MenuPanelFileViewModel FileMenu { get; }

        public MenuPanelViewModel(MenuPanelFileViewModel mpfvm)
        {
            FileMenu = mpfvm;
        }
    }
}
