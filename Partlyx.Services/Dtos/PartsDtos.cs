
namespace Partlyx.Services.Dtos
{
    public record ResourceDto(Guid Uid, string Name, Guid? DefaultRecipeUid, IconDto Icon);

    public record RecipeDto(Guid Uid, string Name, bool IsReversible, List<RecipeComponentDto> Inputs, List<RecipeComponentDto> Outputs, IconDto Icon);

    public record RecipeComponentDto(Guid Uid, Guid? ParentRecipeUid, Guid ResourceUid, double Quantity, bool IsOutput, Guid? SelectedRecipeUid);
}
