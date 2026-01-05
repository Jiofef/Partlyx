using Partlyx.ViewModels.Graph;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.UIStates
{
    public class RecipeComponentPathItemUIState : FocusableItemUIState
    {
        public RecipeComponentPathItem PathItem { get; }

        public override IFocusedElementContainer GlobalFocusedContainer { get; }

        public override IFocusable AttachedFocusable => PathItem;

        public RecipeComponentPathItemUIState(RecipeComponentPathItem pathItem, IFocusedElementContainer focusedContainer)
        {
            PathItem = pathItem;
            GlobalFocusedContainer = focusedContainer;

        }
    }
}
