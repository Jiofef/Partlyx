
namespace Partlyx.Services.Dtos
{
    public record ResourceDto(Guid Uid, string Name, List<RecipeDto> Recipes, Guid? DefaultRecipeUid, IconDto Icon);

    public record RecipeDto(Guid Uid, Guid? ParentResourceUid, string Name, double CraftAmount, List<RecipeComponentDto> Components, IconDto Icon);

    public record RecipeComponentDto(Guid Uid, Guid? ParentRecipeUid, Guid ResourceUid, double Quantity, Guid? SelectedRecipeUid);
}
