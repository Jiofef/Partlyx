using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.Services.ServiceImplementations;
using Partlyx.Services.ServiceInterfaces;
using static Partlyx.Infrastructure.Data.DirectoryManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Tests.InfrastructureTests
{
    public class LoggerTest
    {
        private readonly ILogger _logger;
        public LoggerTest() 
        {
            var services = new ServiceCollection();

            services.AddTransient<IWriter, Writer>();
            services.AddTransient<IReader, Reader>();
            services.AddTransient<ILogger, Logger>();

            var provider = services.BuildServiceProvider();

            _logger = provider.GetRequiredService<ILogger>();
        }

        [Fact]
        public void Log_LogNumber15_Read15FromFile()
        {
            // Arrange
            int num = 15;
            string text = num.ToString();
            var reader = new Reader();

            CreatePartlyxFolder();

            var name = "testlog.txt";
            var dest = Path.Combine(_logger.GetLoggerFolder(), name);

            // Act
            _logger.Log(text, name);

            // Assert
            var writtenText = File.ReadAllText(dest);
            Assert.Equal(text, writtenText);
        }
    }
}
