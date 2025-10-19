using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Core.Contracts
{
    public interface IApplicationResourceProvider
    {
        string GetString(string key, CultureInfo? culture = null);
    }
}
