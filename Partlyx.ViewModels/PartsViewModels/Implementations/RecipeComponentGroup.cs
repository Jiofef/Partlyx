using Partlyx.ViewModels.UIStates;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    // To separate output and input component groups
    public class RecipeComponentGroup
    {
        public string GroupName { get; }
        public ObservableCollection<RecipeComponentViewModel> GroupCollection { get; }
        public RecipeViewModel ParentRecipe { get; }
        public RecipeComponentGroup(string groupName, ObservableCollection<RecipeComponentViewModel> groupCollection, RecipeViewModel parentRecipe)
        {
            GroupName = groupName;
            GroupCollection = groupCollection;
            ParentRecipe = parentRecipe;

            UiItem = new(this);
        }

        public RecipeComponentGroupItemUIState UiItem { get; }
    }
    public class RecipeComponentInputGroup(string GroupName, ObservableCollection<RecipeComponentViewModel> GroupCollection, RecipeViewModel ParentRecipe)
    : RecipeComponentGroup(GroupName, GroupCollection, ParentRecipe);
    public class RecipeComponentOutputGroup(string GroupName, ObservableCollection<RecipeComponentViewModel> GroupCollection, RecipeViewModel ParentRecipe)
        : RecipeComponentGroup(GroupName, GroupCollection, ParentRecipe);
}
