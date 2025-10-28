using Partlyx.Core;
using Partlyx.Core.VisualsInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services.CoreExtensions
{
    public static class IconHolderExtensions
    {
        public static IIcon GetIcon(this IIconHolder holder)
        {
            var info = holder.GetIconInfo();

            return info.GetIcon();
        }
    }
}
