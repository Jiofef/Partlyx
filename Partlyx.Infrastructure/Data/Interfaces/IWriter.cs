namespace Partlyx.Infrastructure.Data.Implementations
{
    public interface IWriter
    {
        void Write(string text, string targetFile);
    }
}