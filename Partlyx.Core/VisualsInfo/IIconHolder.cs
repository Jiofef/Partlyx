namespace Partlyx.Core.VisualsInfo
{
    public interface IIconHolder
    {
        public IconTypeEnum IconType { get; }
        public string IconData { get; }

        IconInfo GetIconInfo();
    }
}
