namespace Partlyx.Services.Dtos
{
    public record RecipeComponentDto(int ResourceId, string ResourceName, double Quantity, int? SelectedRecipeIndex);
    public record RecipeDto(List<RecipeComponentDto> Components);
    public record ResourceDto(int Id, string Name, List<RecipeDto> Recipes, int DefaultRecipeIndex);
}
