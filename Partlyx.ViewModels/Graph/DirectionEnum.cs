namespace Partlyx.ViewModels.Graph
{
    public enum DirectionEnum { Left, Right, Up, Down }
    public static class DirectionExtensions
    {
        public static DirectionEnum TurnBack(this DirectionEnum dir)
        {
            return dir switch
            {
                DirectionEnum.Left => DirectionEnum.Right,
                DirectionEnum.Right => DirectionEnum.Left,
                DirectionEnum.Up => DirectionEnum.Down,
                DirectionEnum.Down => DirectionEnum.Up,
                _ => DirectionEnum.Left,
            };
        }
        public static DirectionEnum TurnLeft(this DirectionEnum dir)
        {
            return dir switch
            {
                DirectionEnum.Left => DirectionEnum.Down,
                DirectionEnum.Right => DirectionEnum.Up,
                DirectionEnum.Up => DirectionEnum.Left,
                DirectionEnum.Down => DirectionEnum.Right,
                _ => DirectionEnum.Left,
            };
        }
        public static DirectionEnum TurnRight(this DirectionEnum dir)
        {
            return dir switch
            {
                DirectionEnum.Left => DirectionEnum.Up,
                DirectionEnum.Right => DirectionEnum.Down,
                DirectionEnum.Up => DirectionEnum.Right,
                DirectionEnum.Down => DirectionEnum.Left,
                _ => DirectionEnum.Left,
            };
        }
    }
}
