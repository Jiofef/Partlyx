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
            Color = StandardVisualSettings.StandardMainPartlyxColor;
            FigureType = FigureTypes.Circle;
        }
        
        public FigureIcon(Color color, string figureType)
        {
            Color = color;
            FigureType = figureType;
        }
    }
}
