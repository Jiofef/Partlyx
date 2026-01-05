using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIStates
{
    public abstract partial class PartItemUIState : FocusableItemUIState, IFindableInTree
    {
        public abstract IVMPart AttachedPart { get; }
        public abstract void FindInTree();
    }
}
