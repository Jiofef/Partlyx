using Partlyx.ViewModels.PartsViewModels.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels
{
    public static class RecipeComponentViewModelExtensions
    {
        public static List<RecipeComponentViewModel> GetSiblings(this RecipeComponentViewModel component)
        {
            var parentRecipe = component.ParentRecipe;

            if (parentRecipe == null)
                return new List<RecipeComponentViewModel>();

            var list = parentRecipe.GetComponents(component.IsOutput).ToList();
            list.Remove(component);
            return list;
        }

        public static List<RecipeComponentViewModel> GetWithSiblings(this RecipeComponentViewModel component)
        {
            var parentRecipe = component.ParentRecipe;

            if (parentRecipe == null)
                return new List<RecipeComponentViewModel>();

            var list = parentRecipe.GetComponents(component.IsOutput).ToList();
            // The only one difference from GetSiblings is that we don't remove the component from its recipe components list
            // list.Remove(component);
            return list;
        }
    }
}
