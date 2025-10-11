using System.Numerics;

namespace Partlyx.ViewModels.GraphicsViewModels
{
    public interface ISizeObject
    {
        float Width { get; }
        float Height { get; }

        public Vector2 GetSize() => new Vector2(Width, Height);
    }
}
