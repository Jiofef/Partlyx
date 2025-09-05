namespace Partlyx.Services
{
    /// <summary>
    /// Facade class for all the parts services
    /// </summary>
    public class PartsService : IPartsService
    {
        public IResourceService Resources { get; }
        public IRecipeService Recipes { get; }
        public IRecipeComponentService RecipeComponents { get; }

        public PartsService(IResourceService rs, IRecipeService rs2, IRecipeComponentService rcs)
        {
            Resources = rs;
            Recipes = rs2;
            RecipeComponents = rcs;
        }
    }
}
