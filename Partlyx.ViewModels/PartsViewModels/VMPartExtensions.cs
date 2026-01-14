using Partlyx.ViewModels.Graph.PartsGraph;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels
{
    public static class VMPartExtensions
    {
        public static RecipeViewModel? GetRelatedRecipe(this IVMPart part)
        {
            if (part is ResourceViewModel resource)
                return resource.LinkedDefaultRecipe?.Value;
            else if (part is RecipeViewModel recipe)
                return recipe;
            else if (part is RecipeComponentViewModel component)
                return component.LinkedParentRecipe?.Value;
            else
                return null;
        }

        public static RecipeViewModel? GetRelatedRecipe(this IFocusable focusable)
        {
            if (focusable is IVMPart part)
                return GetRelatedRecipe(part);
            
            return null;
        }

        public static RecipeComponentPath? GetRelatedRecipeComponentPath(this IFocusable focusable)
        {
            if (focusable is RecipeComponentPathItem pathItem)
                return pathItem.Path;

            return null;
        }
        public static RecipeComponentPath? GetRelatedRecipeComponentPathItem(this IFocusable focusable)
        {
            if (focusable is RecipeComponentPathItem pathItem)
                return pathItem.Path;

            return null;
        }
    }
}
