using System.Drawing;

namespace Partlyx.Core.VisualsInfo
{
    public interface IIcon { }

    public interface IPathIcon : IIcon
    {
        string Path { get; set; }
    }

    public interface IUidIcon : IIcon 
    {
        Guid Uid { get; }
    }

    public interface IFigureIcon : IIcon
    {
        Color Color { get; set; }

        /// <summary>
        /// Must have one of the FigureTypes values
        /// </summary>
        string FigureType { get; set; }
    }

    public enum IconTypeEnum { Null, Figure, Image, Inherited }
}
