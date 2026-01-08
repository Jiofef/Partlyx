using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UJL.CSharp.Collections;

namespace Partlyx.ViewModels.Graph
{
    public class ResourceGraphNodeViewModel : GraphNodeViewModel
    {
        public ResourceGraphNodeViewModel(ResourceViewModel value, GraphNodeViewModel? mainRelative = null)
            : base(mainRelative, value) { }
    }
}
