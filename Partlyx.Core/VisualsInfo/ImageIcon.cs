using System.Text.Json;

namespace Partlyx.Core.VisualsInfo
{
    public class ImageIcon : IUidIcon
    {
        public Guid Uid { get; set; }

        public string Name { get; set; } = "Image";

        public ImageIcon(Guid imageUid)
        {
            Uid = imageUid;
        }
    }
}
