using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Partlyx.Core.VisualsInfo
{
    public class ImageIcon : IPathIcon
    {
        public string Path { get; set; }

        public ImageIcon(string path)
        {
            Path = path;
        }

        public ImageIcon CreateFromIconInfo(IconInfo info)
        {
            if (info.Type != IconTypeEnum.Image)
                throw new ArgumentException("Sent IconInfo does not match ImageIcon type");

            return JsonSerializer.Deserialize<ImageIcon>(info.Data)!;
        }
    }
}
