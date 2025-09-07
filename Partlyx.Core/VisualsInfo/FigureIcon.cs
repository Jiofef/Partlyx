using System.Drawing;
using System.Text.Json;

namespace Partlyx.Core.VisualsInfo
{
    public class FigureIcon : IFigureIcon
    {
        public Color Color { get; set; }
        public string FigureType { get; set; }

        public FigureIcon()
        {
            Color = Color.GreenYellow;
            FigureType = FigureTypes.Circle;
        }
        
        public FigureIcon(Color color, string figureType)
        {
            Color = color;
            FigureType = figureType;
        }

        public FigureIcon CreateFromIconInfo(IconInfo info)
        {
            if (info.Type != IconTypeEnum.Figure)
                throw new ArgumentException("Sent IconInfo does not match FigureIcon type");

            return JsonSerializer.Deserialize<FigureIcon>(info.Data)!;
        }
    }
}
