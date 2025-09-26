using Partlyx.Core.VisualsInfo;
using System.ComponentModel.DataAnnotations.Schema;

namespace Partlyx.Core
{
    public class Resource : ICloneable, IPart
    {
        public Resource(string name = "Resource")
        {
            Name = name;
            _recipes = new List<Recipe>();
        }

        protected Resource() { Uid = Guid.NewGuid(); Name = "Resource"; }

        public Guid Uid { get; private set; }

        // Main features

        public string Name { get; set; }

        public IconTypeEnum IconType { get; private set; }
        public string IconData { get; private set; }
        [NotMapped]
        public IIcon? Icon { get; private set; }
        public void SetIcon(IIcon icon, IconInfo info)
        {
            Icon = icon;
            UpdateIconInfo(info);
        }
        public void UpdateIconInfo(IconInfo info)
        {
            IconType = info.Type;
            IconData = info.Data;
        }

        private readonly List<Recipe> _recipes = new();
        public IReadOnlyList<Recipe> Recipes => _recipes;

        private Guid? _defaultRecipeUid;
        public Guid? DefaultRecipeUid { get => _defaultRecipeUid; set => _defaultRecipeUid = value; }

        private Recipe? _defaultRecipeCached;
        [NotMapped]
        public Recipe? DefaultRecipe 
        {
            get => GetActualDefaultRecipe();
            private set => DefaultRecipeUid = value?.Uid;
        }
        private Recipe? GetActualDefaultRecipe()
        {
            if (_defaultRecipeCached?.Uid != _defaultRecipeUid)
                _defaultRecipeCached = _defaultRecipeUid != null ? GetRecipeByUid((Guid)_defaultRecipeUid) : null;

            return _defaultRecipeCached;
        }

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
        }

        internal void RemoveRecipeFromList(Recipe recipe)
        {
            _recipes.Remove(recipe);
        }

        public bool HasRecipe(Recipe recipe) => _recipes.Contains(recipe);

        public bool HasAnyRecipes()
        {
            return Recipes.Count > 0;
        }

        public void SetDefaultRecipe(Recipe? recipe)
        {
            if (recipe != null && !HasRecipe(recipe)) 
                throw new ArgumentException($"Attempt to set the default recipe that is not in the resource. Name of the resource: {Name}");

            DefaultRecipe = recipe;
        }

        public void DetachAllRecipes()
        {
            foreach (var recipe in Recipes)
                recipe.Detach();
        }

        public Recipe? GetRecipeByUid(Guid uid)
        {
            var recipe = Recipes.FirstOrDefault(recipe => recipe.Uid == uid);
            return recipe;
        }
        public RecipeComponent? GetRecipeComponentByUid(Guid uid)
        {
            var recipe = Recipes.FirstOrDefault(
                rp => rp.Components.Any(c => c.Uid == uid));
            var recipeComponent = recipe?.Components.FirstOrDefault(
                c => c.Uid == uid);

            return recipeComponent;
        }

        public Resource Clone()
        {
            var clone = new Resource();
            clone.Name = Name;
            if (Icon is ICloneable cloneableIcon)
            {
                var iconClone = (IIcon)cloneableIcon.Clone();
                var info = new IconInfo(IconType, IconData);
                clone.SetIcon(iconClone, info);
            }

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
