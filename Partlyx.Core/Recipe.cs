namespace Partlyx.Core
{
    public class Recipe
    {
        public Recipe(Resource parentResource)
        {
            ParentResource = parentResource;
        }

        protected Recipe() { } // EF

        // Main features
        private List<RecipeComponent> _components { get; set; } = new List<RecipeComponent>();
        public IReadOnlyList<RecipeComponent> Components => _components;

        public RecipeComponent AddComponent(Resource componentRes, double quantity)
        {
            var rc = new RecipeComponent(this, componentRes, quantity);
            _components.Add(rc);
            return rc;
        }

        public bool RemoveComponent(RecipeComponent component) => _components.Remove(component);


        // Secondary features
        public Resource ParentResource { get; private set; }

        /// <summary>
        /// Returns all fundamental components of the recipe
        /// </summary>
        public Recipe Quantify()
        {
            Recipe quantizedRecipe = new Recipe(ParentResource);

            TakeToPieces(this);

            void TakeToPieces(Recipe recipe)
            {
                foreach (var component in recipe._components)
                {
                    var resource = component.ComponentResource;
                    if (resource.HasAnyRecipes())
                        TakeToPieces(component.ComponentSelectedRecipe);
                    else
                        component.CopyTo(quantizedRecipe);
                }
            }

            return quantizedRecipe;
        }
    }
}
