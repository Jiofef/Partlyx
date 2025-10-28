namespace Partlyx.Core.VisualsInfo
{
    public class IconInfo
    {
        public IconTypeEnum Type { get; set; }
        public string Data { get; set; }

        public IconInfo(IconTypeEnum type, string data) 
        {
            Type = type;
            Data = data;
        }
    }
}
