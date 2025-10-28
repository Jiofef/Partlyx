using Partlyx.Core.VisualsInfo;
using System.Text.Json;

namespace Partlyx.Services.CoreExtensions
{
    public static class IconInfoExtensions
    {
        public static IIcon GetIcon(this IconInfo info)
        {
            var nullIcon = new NullIcon();
            if (info.Data == null) return nullIcon;

            IIcon? result = null;

            switch (info.Type)
            {
                case IconTypeEnum.Image:
                    result = JsonSerializer.Deserialize<ImageIcon>(info.Data);
                    break;
                case IconTypeEnum.Figure:
                    result = JsonSerializer.Deserialize<FigureIcon>(info.Data);
                    break;
            }

            if (result == null)
                result = nullIcon;

            return result;
        }
    }
}
