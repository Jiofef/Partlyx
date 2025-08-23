namespace Partlyx.Services.Dtos
{
    public record RecipeComponentDto(Guid ResourceUid, string ResourceName, double Quantity, Guid? SelectedRecipeUid);
    public record RecipeDto(Guid Uid, double CraftAmount, List<RecipeComponentDto> Components);
    public record ResourceDto(Guid Uid, string Name, List<RecipeDto> Recipes, Guid? DefaultRecipeUid);
}
