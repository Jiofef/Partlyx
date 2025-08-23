using System.ComponentModel.DataAnnotations.Schema;

namespace Partlyx.Core
{
    public class Resource : ICloneable
    {
        public Resource(string name = "Resource")
        {
            Name = name;
            _recipes = new List<Recipe>();
        }

        protected Resource() { Uid = new Guid(); }

        public Guid Uid { get; private set; }

        // Main features

        public string Name { get; set; }

        private readonly List<Recipe> _recipes = new();
        public IReadOnlyList<Recipe> Recipes => _recipes;

        private Guid? _defaultRecipeUid;
        public Guid? DefaultRecipeUid
        {
            get => _defaultRecipeUid;
            set
            {
                _defaultRecipeUid = value;
                // Trying to resolve DefaultRecipe if recipes already loaded
                DefaultRecipe = value.HasValue ? _recipes.FirstOrDefault(r => r.Uid == value.Value) : null;
            }
        }

        [NotMapped]
        public Recipe DefaultRecipe { get; private set; }

        // Secondary features
        public Recipe CreateRecipe()
        {
            var r = Recipe.CreateForResource(this);

            AddRecipeToList(r);

            return r;
        }

        internal void AddRecipeToList(Recipe recipe)
        {
            _recipes.Add(recipe);

            // If the added recipe is the only one
            if (_recipes.Count == 1)
                SetDefaultRecipe(recipe);
        }

        internal void RemoveRecipeFromList(Recipe recipe)
        {
            _recipes.Remove(recipe);
            if (DefaultRecipe == recipe)
            {
                if (_recipes.Count > 0)
                    SetDefaultRecipe(_recipes[0]);
                else
                    SetDefaultRecipe(null);
            }
        }

        public bool HasRecipe(Recipe recipe) => _recipes.Contains(recipe);

        public bool HasAnyRecipes()
        {
            return Recipes.Count > 0;
        }

        public void SetDefaultRecipe(Recipe? recipe)
        {
            if (!HasRecipe(recipe)) 
                throw new ArgumentException($"Attempt to set the default recipe that is not in the resource. Name of the resource: {Name}");

            DefaultRecipe = recipe;
            _defaultRecipeUid = recipe != null ? recipe.Uid : null;
        }

        public void DetachAllRecipes()
        {
            foreach (var recipe in Recipes)
                recipe.Detach();
        }


        public Resource Clone()
        {
            var clone = new Resource();
            foreach (var recipe in _recipes)
            {
                if (DefaultRecipe != recipe)
                    recipe.CopyTo(clone);
                else
                {
                    var newDefaultRecipe = recipe.CopyTo(clone);
                    clone.SetDefaultRecipe(newDefaultRecipe);
                }
            }


            return clone;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
