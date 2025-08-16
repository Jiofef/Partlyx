namespace Partlyx.Core
{
    public class RecipeComponent : ICopiable<Recipe>
    {
        public RecipeComponent(Recipe parentRecipe, Resource component, double quantity)
        {
            ParentRecipe = parentRecipe;
            ComponentResource = component;
            Quantity = quantity;
        }

        protected RecipeComponent() { } // EF

        // Main features
        public Resource ComponentResource { get; }

        public double Quantity { get; private set; }

        // Secondary features
        public Recipe ParentRecipe { get; private set; }

        public bool IsOutput() => Quantity < 0;

        private Recipe? _componentSelectedRecipe = null; // null means default value for recipe. See Recipe.ComponentDefaultRecipe

        public Recipe ComponentSelectedRecipe
        {
            get => _componentSelectedRecipe ?? ComponentResource.ComponentDefaultRecipe;
        }

        public void SetSelectedRecipe(Recipe? recipe)
        {
            if (recipe is null)
            {
                _componentSelectedRecipe = null;
                return;
            }

            if (!ComponentResource.HasRecipe(recipe)) 
                throw new ArgumentException($"Attempt to set the recipe that is not in the resource. Name of the resource: {ComponentResource.Name}");

            _componentSelectedRecipe = recipe;
        }

        /// <summary>
        /// Returns a newly created copy of the component in the specified recipe
        /// </summary>
        public RecipeComponent CopyTo(Recipe recipe)
        {
            var copy = recipe.AddComponent(ComponentResource, Quantity);
            copy.SetSelectedRecipe(_componentSelectedRecipe);

            return copy;
        }
        ICopiable<Recipe> ICopiable<Recipe>.CopyTo(Recipe recipe)
        {
            return CopyTo(recipe);
        }
    }
}
