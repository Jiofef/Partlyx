using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.UIStates
{
    public class RecipeComponentGroupItemUIState : ItemUIState
    {
        public RecipeComponentGroupItemUIState(RecipeComponentGroup attachedGroup)
        {
            _attachedGroup = attachedGroup;

            IsExpanded = true;
        }
        private readonly RecipeComponentGroup _attachedGroup;
        public override object AttachedObject => _attachedGroup;
    }
}
