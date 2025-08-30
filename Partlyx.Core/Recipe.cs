﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Partlyx.Core
{
    public class Recipe : ICopiable<Resource>, IPart
    {
        // Hierarchy
        public Resource? ParentResource { get; private set; }

        public bool IsDetached => ParentResource == null;

        public static Recipe CreateForResource(Resource parentResource)
        {
            var recipe = new Recipe()
            {
                ParentResource = parentResource,
            };

            return recipe;
        }
        public static Recipe CreateDetached()
        {
            var recipe = new Recipe();

            return recipe;
        }

        public void Detach()
        {
            if (ParentResource != null)
                ParentResource.RemoveRecipeFromList(this);

            ParentResource = null;
        }
        public void AttachTo(Resource resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            if (ParentResource != null)
                ParentResource.RemoveRecipeFromList(this);

            resource.AddRecipeToList(this);
            ParentResource = resource;
        }

        internal void AddRecipeComponentToList(RecipeComponent component) => _components.Add(component);

        internal bool RemoveRecipeComponentFromList(RecipeComponent component) => _components.Remove(component);

        protected Recipe() { Uid = new Guid(); }

        public Guid Uid { get; private set; }

        // Main features
        private List<RecipeComponent> _components { get; set; } = new List<RecipeComponent>();
        public IReadOnlyList<RecipeComponent> Components => _components;
        public double CraftAmount { get; set; } = 1;

        public RecipeComponent CreateComponent(Resource componentRes, double quantity)
        {
            var rc = RecipeComponent.CreateForRecipe(this, componentRes, quantity);
            _components.Add(rc);
            return rc;
        }

        public bool RemoveComponent(RecipeComponent component) => _components.Remove(component);

        /// <summary>
        /// Clears the list of components
        /// </summary>
        public void Clear() => _components.Clear();

        // Secondary features
        /// <summary>
        /// Breaks down the recipe to its fundamental components
        /// </summary>
        public void MakeQuantified()
        {
            var quants = new List<RecipeComponent>();

            void TakeToPieces(Recipe recipe, double factor)
            {
                foreach (var component in recipe.Components)
                {
                    var totalQuantity = component.Quantity * factor;

                    if (component.ComponentResource.HasAnyRecipes())
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
                _components.Add(c);
        }


        public void MergeDuplicateComponents()
        {
            var groups = new Dictionary<(Resource, Recipe), List<RecipeComponent>>();

            foreach (var component in _components)
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
            var component = Components.FirstOrDefault(component => component.Uid == uid);
            return component;
        }

        /// <summary>
        /// Returns a newly created copy of the component in the specified recipe
        /// </summary>
        public Recipe CopyTo(Resource resource)
        {
            var copy = CloneDetached();

            resource.AddRecipeToList(copy);

            return copy;
        }
        ICopiable<Resource> ICopiable<Resource>.CopyTo(Resource resource)
        {
            return CopyTo(resource);
        }

        public Recipe CloneDetached()
        {
            var clone = CreateDetached();

            foreach (var component in _components)
                component.CopyTo(clone);

            return clone;
        }
    }
}
