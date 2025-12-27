using Partlyx.Core.VisualsInfo;
using System.ComponentModel.DataAnnotations.Schema;

namespace Partlyx.Core.Partlyx
{
    public class Resource : ICloneable, IIconHolder, IPart
    {
        public Resource(string name = "Resource")
        {
            Name = name;
        }

        protected Resource() { Uid = Guid.NewGuid(); Name = "Resource"; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Uid { get; private set; }

        // Main features

        public string Name { get; set; }

        private Guid? _defaultRecipeUid;
        public Guid? DefaultRecipeUid { get => _defaultRecipeUid; set => _defaultRecipeUid = value; }

        private Recipe? _defaultRecipeCached;

        [NotMapped]
        public Recipe? DefaultRecipe
        {
            get => _defaultRecipeCached;
            set
            {
                _defaultRecipeCached = value;
                _defaultRecipeUid = value?.Uid;
            }
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

        public void SetDefaultRecipe(Recipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            bool hasResource = recipe.IsReversible
                ? recipe.Components.Any(c => c.ComponentResource.Uid == this.Uid)
                : recipe.Outputs.Any(c => c.ComponentResource.Uid == this.Uid);
            if (!hasResource)
            {
                throw new ArgumentException("Recipe does not produce this resource");
            }
            DefaultRecipe = recipe;
        }

        public RecipeComponent? GetRecipeComponentByUid(Guid uid)
        {
            // Since recipes are not tied to resources, this method may need to be removed or changed
            // For now, leave as stub
            return null;
        }

        public Resource Clone()
        {
            var clone = new Resource();
            clone.Name = Name;

            clone.IconType = IconType;
            clone.IconData = IconData;

            clone.DefaultRecipe = DefaultRecipe;

            return clone;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
