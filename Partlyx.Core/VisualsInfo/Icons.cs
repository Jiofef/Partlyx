using System.Drawing;

namespace Partlyx.Core.VisualsInfo
{
    public interface IIcon { }

    public interface IPathIcon : IIcon
    {
        string Path { get; set; }
    }

    public interface IFigureIcon : IIcon
    {
        Color Color { get; set; }

        /// <summary>
        /// Must have one of the FigureTypes values
        /// </summary>
        string FigureType { get; set; }
    }

    public enum IconTypeEnum { Figure, Image }
}
