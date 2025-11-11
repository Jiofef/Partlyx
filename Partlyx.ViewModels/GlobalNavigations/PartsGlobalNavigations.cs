using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.GlobalNavigations
{
    public class PartsGlobalNavigations
    {
        public IGlobalFocusedPart FocusedPart { get; }
        public IGlobalSelectedParts SelectedParts { get; }
        public PartsGlobalNavigations(IGlobalFocusedPart focusedPart, IGlobalSelectedParts selectedParts) 
        {
            FocusedPart = focusedPart;
            SelectedParts = selectedParts;
        }
    }
}
