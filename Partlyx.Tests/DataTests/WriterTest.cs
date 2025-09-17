using Partlyx.Infrastructure.Data;
using static Partlyx.Infrastructure.Data.DirectoryManager;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Partlyx.Infrastructure.Data.Implementations;

namespace Partlyx.Tests.DataTests
{
    public class WriterTest
    {
        [Fact]
        public void Write_WriteSumOf5And5_Get10FromFile()
        {
            // Arrange
            int num = 5 + 5;
            string text = num.ToString();
            var writer = new Writer();

            CreatePartlyxFolder();

            var name = "testfile";
            var dest = GetInPartlyxFolder(name);

            // Act
            writer.Write(text, dest);

            // Assert
            var writtenText = File.ReadAllText(dest);
            Assert.Equal(text, writtenText);
        }
    }
}
