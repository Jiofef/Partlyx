using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public class Reader : IReader
    {
        public Reader() { }

        public string Read(string targetFile)
        {
            return File.ReadAllText(targetFile);
        }
    }
}
