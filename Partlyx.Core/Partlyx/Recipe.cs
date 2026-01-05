using Partlyx.Core.VisualsInfo;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Partlyx.Core.Partlyx
{
    public class Recipe : IIconHolder, IPart
    {
        public static Recipe Create(string name = "Recipe")
        {
            var recipe = new Recipe()
            {
                Name = name,
            };

            return recipe;
        }

        internal void AddRecipeComponentToList(RecipeComponent component)
        {
            Components.Add(component);
        }

        internal bool RemoveRecipeComponentFromList(RecipeComponent component)
        {
            return Components.Remove(component);
        }

        protected Recipe() { Uid = Guid.NewGuid(); Name = "Recipe"; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Uid { get; private set; }

        // Main features
        public string Name { get; set; }
        public virtual List<RecipeComponent> Components { get; } = new List<RecipeComponent>();

        [NotMapped]
        public IReadOnlyList<RecipeComponent> Inputs => Components.Where(c => !c.IsOutput).ToList();
        [NotMapped]
        public IReadOnlyList<RecipeComponent> Outputs => Components.Where(c => c.IsOutput).ToList();
        public bool IsReversible { get; set; } = false;

        public RecipeComponent CreateInput(Resource componentRes, double quantity)
        {
            var rc = RecipeComponent.CreateForRecipe(this, componentRes, quantity);
            rc.IsOutput = false;
            Components.Add(rc);
            return rc;
        }

        public RecipeComponent CreateOutput(Resource componentRes, double quantity)
        {
            var rc = RecipeComponent.CreateForRecipe(this, componentRes, quantity);
            rc.IsOutput = true;
            Components.Add(rc);
            return rc;
        }

        public RecipeComponent CreateComponent(Resource componentRes, double quantity)
        {
            var rc = RecipeComponent.CreateForRecipe(this, componentRes, quantity);
            Components.Add(rc);
            return rc;
        }

        public bool RemoveComponent(RecipeComponent component)
        {
            return Components.Remove(component);
        }

        /// <summary>
        /// Clears the list of components
        /// </summary>
        public void Clear()
        {
            Components.Clear();
        }

        // Icon features
        public IconTypeEnum IconType { get; private set; }
        public string IconData { get; private set; } = "{}";
        public void UpdateIconInfo(IconInfo info)
        {
            IconType = info.Type;
            IconData = info.Data;
        }
        public IconInfo GetIconInfo() => new IconInfo(IconType, IconData);

        // Secondary features
        /// <summary>
        /// Breaks down the recipe to its fundamental components
        /// </summary>
        public void MakeQuantified()
        {
            var quants = new List<RecipeComponent>();

            void TakeToPieces(Recipe recipe, double factor)
            {
                foreach (var component in recipe.Inputs)
                {
                    var totalQuantity = component.Quantity * factor;

                    if (component.ComponentSelectedRecipe != null)
                    {
                        TakeToPieces(component.ComponentSelectedRecipe, totalQuantity);
                    }
                    else
                    {
                        quants.Add(
                            RecipeComponent.CreateForRecipe(this, component.ComponentResource, totalQuantity)
                        );
                    }
                }
            }

            TakeToPieces(this, 1.0);

            Clear();
            foreach (var c in quants)
                Components.Add(c);
        }


        public void MergeDuplicateComponents()
        {
            var groups = new Dictionary<(Resource, Recipe?), List<RecipeComponent>>();

            foreach (var component in Components)
            {
                var key = (component.ComponentResource, component.ComponentSelectedRecipe);
                if (!groups.TryGetValue(key, out var list))
                {
                    list = new List<RecipeComponent>();
                    groups[key] = list;
                }
                list.Add(component);
            }

            foreach (var group in groups.Values)
            {
                if (group.Count > 1)
                {
                    double sum = group.Sum(c => c.Quantity);
                    group[0].Quantity = sum;
                    for (int i = 1; i < group.Count; i++)
                    {
                        RemoveComponent(group[i]);
                    }
                }
            }
        }


        public RecipeComponent? GetRecipeComponentByUid(Guid uid)
        {
            var component = Inputs.FirstOrDefault(component => component.Uid == uid);
            if (component != null) return component;
            component = Outputs.FirstOrDefault(component => component.Uid == uid);
            return component;
        }

        public Recipe Clone()
        {
            var clone = Create();
            clone.Name = Name;
            clone.IsReversible = IsReversible;

            clone.IconType = IconType;
            clone.IconData = IconData;

            foreach (var component in Components)
                component.CopyTo(clone);

            return clone;
        }
    }
}
