using Partlyx.Core.Partlyx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Partlyx.Services.CoreExtensions.RecipeExtensions;

namespace Partlyx.Services.CoreExtensions
{
    public static class RecipeExtensions
    {
        /// <summary> Checks if the resource is present in the recipe's inputs or outputs </summary>
        public static bool HasResource(this Recipe recipe, Guid resourceUid)
        {
            return recipe.HasResourceInInputs(resourceUid) || recipe.HasResourceInOutputs(resourceUid);
        }

        /// <summary>
        /// Checks if the resource is present in the recipe's inputs
        /// </summary>
        public static bool HasResourceInInputs(this Recipe recipe, Guid resourceUid)
        {
            return recipe.Inputs.Any(c => c.ComponentResource.Uid == resourceUid);
        }

        /// <summary> Checks if the resource is present in the recipe's outputs </summary>
        public static bool HasResourceInOutputs(this Recipe recipe, Guid resourceUid)
        {
            return recipe.Outputs.Any(c => c.ComponentResource.Uid == resourceUid);
        }

        /// <summary>
        /// Checks if the resource is present in the recipe's components that can be used as default recipe
        /// If recipe is reversible, checks both inputs and outputs; otherwise, only outputs 
        /// </summary>
        public static bool HasResourceInLinkedComponents(this Recipe recipe, Guid resourceUid)
        {
            if (recipe.IsReversible)
                return recipe.HasResource(resourceUid);
            else
                return recipe.HasResourceInOutputs(resourceUid);
        }

        public static List<RecipeComponent> ComponentsWithResource(this Recipe recipe, Guid resourceUid)
        {
            var list = new List<RecipeComponent>();
            list.AddRange(recipe.InputsWithResource(resourceUid));
            list.AddRange(recipe.OutputsWithResource(resourceUid));
            return list;
        }

        public static List<RecipeComponent> InputsWithResource(this Recipe recipe, Guid resourceUid)
        {
            return recipe.Inputs.Where(c => c.ComponentResource.Uid == resourceUid).ToList();
        }

        public static List<RecipeComponent> OutputsWithResource(this Recipe recipe, Guid resourceUid)
        {
            return recipe.Outputs.Where(c => c.ComponentResource.Uid == resourceUid).ToList();
        }

        public static List<RecipeComponent> LinkedComponentsWithResource(this Recipe recipe, Guid resourceUid)
        {
            if (recipe.IsReversible)
                return recipe.ComponentsWithResource(resourceUid);
            else
                return recipe.OutputsWithResource(resourceUid);
        }
    }
}
