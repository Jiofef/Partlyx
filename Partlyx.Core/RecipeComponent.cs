namespace Partlyx.Core
{
    public class RecipeComponent : ICopiable<Recipe>
    {
        // Hierarchy
        public Recipe? ParentRecipe { get; private set; }
        public bool IsDetached => ParentRecipe == null;

        public static RecipeComponent CreateForRecipe(Recipe parentRecipe, Resource component, double quantity)
        {
            var recipeComponent = new RecipeComponent()
            {
                ParentRecipe = parentRecipe,
                ComponentResource = component,
                Quantity = quantity,
            };

            return recipeComponent;
        }

        public static RecipeComponent CreateDetached()
        {
            var recipeComponent = new RecipeComponent();

            return recipeComponent;
        }

        public void Detach()
        {
            if (ParentRecipe != null)
                ParentRecipe.RemoveComponent(this);

            ParentRecipe = null;
        }
        public void AttachTo(Recipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));


            if (ParentRecipe != null)
                ParentRecipe.RemoveComponent(this);

            recipe.AddRecipeComponentToList(this);
            ParentRecipe = recipe;
        }


        protected RecipeComponent() { } // EF

        // Main features
        public Resource ComponentResource { get; private set; }

        public double Quantity { get; set; }

        // Secondary features

        public bool IsOutput() => Quantity < 0;

        private Recipe? _componentSelectedRecipe = null; // null means default value for recipe. See Resource.DefaultRecipe

        public Recipe ComponentSelectedRecipe
        {
            get => _componentSelectedRecipe ?? ComponentResource.DefaultRecipe;
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

        public void Remove() => ParentRecipe?.RemoveComponent(this);

        /// <summary>
        /// Returns a newly created copy of the component in the specified recipe
        /// </summary>
        public RecipeComponent CopyTo(Recipe recipe)
        {
            var copy = CloneDetached();
            recipe.AddRecipeComponentToList(copy);

            return copy;
        }
        ICopiable<Recipe> ICopiable<Recipe>.CopyTo(Recipe recipe)
        {
            return CopyTo(recipe);
        }
    
        public RecipeComponent CloneDetached()
        {
            var clone = CreateDetached();

            clone.ComponentResource = ComponentResource;
            clone.Quantity = Quantity;
            clone.SetSelectedRecipe(_componentSelectedRecipe);

            return clone;
        }
    }
}
