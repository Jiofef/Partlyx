namespace Partlyx.Services.ServiceInterfaces
{
    public interface ILogger
    {
        void EnsureLogFolderCreated();
        string GetLoggerFolder();
        void Log(string message, string fileName = "log.txt");
        string ReadLog(string fileName);
        void SetLoggerFolder(string loggerFolder);
    }
}