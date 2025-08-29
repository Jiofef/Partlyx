using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels
{
    public class MainViewModel
    {
        public ResourceListViewModel ResourceList { get; }
        public RecipeListViewModel RecipeEditor { get; }
        public RecipeComponentsViewModel RecipeComponents { get; }

        public PartsTreeViewModel PartsTree { get; }

        public MainViewModel(
            ResourceListViewModel resourceList, RecipeListViewModel recipeEditor, 
            RecipeComponentsViewModel recipeComponents, PartsTreeViewModel partsTree
            )
        {
            ResourceList = resourceList;
            RecipeEditor = recipeEditor;
            RecipeComponents = recipeComponents;
            PartsTree = partsTree;
        }
    }
}
