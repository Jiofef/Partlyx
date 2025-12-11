using Partlyx.Core.VisualsInfo;
using Newtonsoft.Json;

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
                case IconTypeEnum.Null:
                    result = null;
                    break;
                case IconTypeEnum.Image:
                    result = JsonConvert.DeserializeObject<ImageIcon>(info.Data);
                    break;
                case IconTypeEnum.Figure:
                    result = JsonConvert.DeserializeObject<FigureIcon>(info.Data);
                    break;
                case IconTypeEnum.Inherited:
                    result = JsonConvert.DeserializeObject<InheritedIcon>(info.Data);
                    break;
            }

            if (result == null)
                result = nullIcon;

            return result;
        }
    }
}
