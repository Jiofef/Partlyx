using Partlyx.Core.VisualsInfo;
using System.Drawing;
namespace Partlyx.Services.Dtos
{
    public abstract record IconDto();

    public record NullIconDto() : IconDto;

    public record ImageIconDto(Guid ImageUid) : IconDto;

    public record FigureIconDto(Color Color, string FigureType) : IconDto;

    public record InheritedIconDto(Guid ParentUid, InheritedIcon.InheritedIconParentTypeEnum ParentType) : IconDto;
}