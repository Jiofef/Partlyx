namespace Partlyx.ViewModels.GraphicsViewModels
{
    public interface ISizePositionObject : ISizeObject, IPositionObject
    {
        public float XCentered { get => X + Width / 2; set => X = value - Width / 2; }
        public float YCentered { get => Y + Height / 2; set => Y = value - Height / 2; }
    }

}
