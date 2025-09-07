namespace Partlyx.Services.ServiceInterfaces
{
    public interface IPartsService
    {
        IRecipeComponentService RecipeComponents { get; }
        IRecipeService Recipes { get; }
        IResourceService Resources { get; }
    }
}