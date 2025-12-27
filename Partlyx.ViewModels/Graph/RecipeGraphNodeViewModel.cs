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
    public class RecipeGraphNodeViewModel : GraphTreeNodeViewModel, ITypedVMPartHolder<RecipeViewModel>
    {
        public RecipeGraphNodeViewModel(RecipeViewModel value) : base(value.Uid,
                  new ObservableCollectionProjection<Guid, RecipeComponentViewModel>(value.Inputs!, (component => component.Uid)),
                  value) { }

        public PartTypeEnumVM? PartType => PartTypeEnumVM.Recipe;

        public RecipeViewModel? Part => Value as RecipeViewModel;
    }
}
