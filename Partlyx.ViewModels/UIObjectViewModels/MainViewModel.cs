using Partlyx.ViewModels.PartsViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public class MainViewModel
    {
        public IGlobalSelectedParts SelectedParts { get; }
        public ResourceListViewModel ResourceList { get; }
        public RecipeListViewModel RecipeList { get; }
        public RecipeComponentListViewModel RecipeComponents { get; }

        public PartsTreeViewModel PartsTree { get; }

        public MainViewModel(
            ResourceListViewModel resourceList, RecipeListViewModel recipeList, 
            RecipeComponentListViewModel recipeComponents, PartsTreeViewModel partsTree,
            IGlobalSelectedParts selectedParts
            )
        {
            ResourceList = resourceList;
            RecipeList = recipeList;
            RecipeComponents = recipeComponents;
            PartsTree = partsTree;
            SelectedParts = selectedParts;
        }
    }
}
