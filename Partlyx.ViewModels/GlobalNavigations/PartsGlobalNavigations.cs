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
        public IGlobalFocusedElementContainer FocusedElementContainer { get; }
        public IGlobalSelectedParts SelectedParts { get; }
        public PartsGlobalNavigations(IGlobalFocusedElementContainer focusedPart, IGlobalSelectedParts selectedParts) 
        {
            FocusedElementContainer = focusedPart;
            SelectedParts = selectedParts;
        }
    }
}
