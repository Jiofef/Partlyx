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
    public class ComponentGraphNodeViewModel : GraphTreeNodeViewModel
    {
        public ComponentGraphNodeViewModel(RecipeComponentViewModel value) 
            : base(value.Uid,
                  (value.SelectedRecipeComponents != null 
                  ? new ObservableCollectionProjection<Guid, RecipeComponentViewModel>(value.SelectedRecipeComponents, (component => component.Uid)) 
                  : null),
                  value) { }
    }
}
