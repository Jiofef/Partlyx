using System.Security.Cryptography;

namespace Partlyx.Services.Helpers
{
    public static class ByteHelper
    {
        public static byte[] ComputeHash(byte[] data)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(data);
        }
    }
}
