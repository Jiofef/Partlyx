using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Core.Settings
{
    public class OptionEntity
    {
        public int Id { get; set; }
        public string Key { get; set; } = null!;
        public string ValueJson { get; set; } = "{}";
        public string TypeName { get; set; } = "";
    }
}
