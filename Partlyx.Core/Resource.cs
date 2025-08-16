namespace Partlyx.Core
{
    public class Resource
    {
        public Resource()
        {
            Name = "";
            _recipes = new List<Recipe>();
        }

        // Main features
        public int Id { get; set; }

        public string Name { get; set; }

        private readonly List<Recipe> _recipes = new();
        public IReadOnlyList<Recipe> Recipes => _recipes;

        public Recipe ComponentDefaultRecipe { get; private set; }

        // Secondary features
        public Recipe CreateRecipe()
        {
            var r = new Recipe(this);
            _recipes.Add(r);
            return r;
        }

        public bool RemoveRecipe(Recipe recipe) => _recipes.Remove(recipe);

        public bool HasRecipe(Recipe recipe) => _recipes.Contains(recipe);

        public void SetDefaultRecipe(Recipe recipe)
        {
            if (!HasRecipe(recipe)) 
                throw new ArgumentException($"Attempt to set the default recipe that is not in the resource. Name of the resource: {Name}");

            ComponentDefaultRecipe = recipe;
        }

        public void ClearRecipes() => _recipes.Clear();

        public bool HasAnyRecipes()
        {
            return Recipes.Count > 0;
        }
    }
}
