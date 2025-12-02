namespace Partlyx.ViewModels.GraphicsViewModels
{
    public struct Directions4
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;

        public Directions4(float all)
        {
            Left = all;
            Right = all;
            Top = all;
            Bottom = all;
        }

        public Directions4(float x, float y)
        {
            Left = x;
            Right = x;
            Top = y;
            Bottom = y;
        }
        public Directions4(float left, float right, float top, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
    }
}
