namespace Partlyx.Services.Dtos
{
    public record RecipeComponentDto(Guid Uid, Guid? ParentRecipeUid, Guid ResourceUid, double Quantity, Guid? SelectedRecipeUid);
    public record RecipeDto(Guid Uid, Guid? ParentResourceUid, double CraftAmount, List<RecipeComponentDto> Components);
    public record ResourceDto(Guid Uid, string Name, List<RecipeDto> Recipes, Guid? DefaultRecipeUid);
}
