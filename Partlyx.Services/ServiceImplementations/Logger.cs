using Partlyx.Infrastructure.Data;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.Services.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services.ServiceImplementations
{
    public class Logger : ILogger
    {
        private readonly IWriter _writer;
        private readonly IReader _reader;

        private string _loggerFolder;

        public Logger(IWriter w, IReader r)
        {
            _writer = w;
            _reader = r;

            _loggerFolder = DirectoryManager.GetInPartlyxFolder("logs\\");

            EnsureLogFolderCreated();
        }

        public void SetLoggerFolder(string loggerFolder) => _loggerFolder = loggerFolder;
        public string GetLoggerFolder() => _loggerFolder;

        public void EnsureLogFolderCreated()
        {
            if (Directory.Exists(_loggerFolder)) return;

            Directory.CreateDirectory(_loggerFolder);
        }

        public void Log(string message, string fileName = "log.txt")
        {
            _writer.Write(message, Path.Combine(_loggerFolder, fileName));
        }

        public string ReadLog(string fileName)
        {
            return _reader.Read(Path.Combine(_loggerFolder, fileName));
        }
    }
}
