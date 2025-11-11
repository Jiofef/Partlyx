using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services.Helpers
{
    public static class BoolHelper
    {
        public static int GetTrueBooleansAmount(params bool[] booleans)
        {
            int result = 0;
            foreach (bool b in booleans)
                if (b) result++;
            return result;
        }

        public static int GetFalseBooleansAmount(params bool[] booleans)
        {
            int result = 0;
            foreach (bool b in booleans)
                if (!b) result++;
            return result;
        }
    }
}
