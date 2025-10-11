using System.ComponentModel.DataAnnotations.Schema;

namespace Partlyx.Core
{
    public class RecipeComponent : ICopiable<Recipe>, IPart
    {
        // Hierarchy
        private Recipe? _parentRecipe;
        public Recipe? ParentRecipe { get => _parentRecipe; private set => _parentRecipe = value; }
        public bool IsDetached => ParentRecipe == null;

        public static RecipeComponent CreateForRecipe(Recipe parentRecipe, Resource component, double quantity)
        {
            var recipeComponent = new RecipeComponent()
            {
                ParentRecipe = parentRecipe,
                ComponentResource = component,
                Quantity = quantity
            };

            return recipeComponent;
        }

        public static RecipeComponent CreateDetached(Resource resourceComponent)
        {
            var recipeComponent = new RecipeComponent();
            recipeComponent.SetComponentResource(resourceComponent);

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


        protected RecipeComponent() { Uid = Guid.NewGuid(); }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Uid { get; private set; }

        // Main features
        private Resource _componentResource;
        public Resource ComponentResource { get => _componentResource; private set => _componentResource = value; }

        public double Quantity { get; set; }

        public void SetComponentResource(Resource resource)
        {
            ComponentResource = resource;

            ComponentSelectedRecipeUid = null;
        }

        // Secondary features
        public bool IsOutput() => Quantity < 0;

        private Guid? _componentSelectedRecipeUid;
        public Guid? ComponentSelectedRecipeUid
        {
            get => _componentSelectedRecipeUid;
            set
            {
                _componentSelectedRecipeUid = value;
                // Trying to resolve DefaultRecipe if recipes already loaded
                _componentSelectedRecipe = value.HasValue ? ComponentResource.Recipes.FirstOrDefault(r => r.Uid == value.Value) : null;
            }
        }

        [NotMapped]
        private Recipe? _componentSelectedRecipe = null; // Null means default value for recipe. See Resource.DefaultRecipe

        [NotMapped]
        public Recipe? ComponentSelectedRecipe // Must be null when resource doesn't contain any recipes
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
            _componentSelectedRecipeUid = recipe != null ? recipe.Uid : null;
        }

        public void Remove() => ParentRecipe?.RemoveComponent(this);

        /// <summary>
        /// Returns a newly created copy of the component in the specified recipe
        /// </summary>
        public RecipeComponent CopyTo(Recipe recipe)
        {
            var copy = CloneDetached();

            copy.ParentRecipe = recipe;
            recipe.AddRecipeComponentToList(copy);

            return copy;
        }
        ICopiable<Recipe> ICopiable<Recipe>.CopyTo(Recipe recipe)
        {
            return CopyTo(recipe);
        }
    
        public RecipeComponent CloneDetached()
        {
            var clone = CreateDetached(ComponentResource);

            clone.Quantity = Quantity;
            clone.SetSelectedRecipe(_componentSelectedRecipe);

            return clone;
        }
    }
}
