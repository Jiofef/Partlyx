using System.Drawing;
namespace Partlyx.Services.Dtos
{
    public abstract record IconDto();

    public record NullIconDto() : IconDto;

    public record ImageIconDto(Guid ImageUid, string ImageName) : IconDto;

    public record FigureIconDto(Color Color, string FigureType) : IconDto;
}