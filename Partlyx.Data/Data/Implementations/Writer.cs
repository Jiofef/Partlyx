using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public class Writer : IWriter
    {
        public Writer() { }

        public void Write(string text, string targetFile)
        {
            File.WriteAllText(targetFile, text);
        }
    }
}
