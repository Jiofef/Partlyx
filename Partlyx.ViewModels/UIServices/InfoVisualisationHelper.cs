using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices
{
    public static class InfoVisualisationHelper
    {
        public static string GetMappingForFloatsString(int decimalPlaces)
        {
            if (decimalPlaces <= 0)
            {
                return "0";
            }

            return "0." + new string('#', decimalPlaces);
        }
    }
}
