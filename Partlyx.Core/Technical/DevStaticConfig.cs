using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Core.Technical
{
    public static class DevStaticConfig
    {
        public const bool ENABLE_VISUAL_UNHANDLED_EXCEPTIONS = true;

        // During development, it is inconvenient to reopen the file every time you restart. However, don't forget to disable this when releasing the application.
        public const bool DISABLE_DB_DELETE_ON_EXIT = false;
    }
}
