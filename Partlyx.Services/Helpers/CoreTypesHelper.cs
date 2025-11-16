using Partlyx.Core.Technical;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Partlyx.Services.Helpers
{
    public static class CoreTypesHelper
    {
        public static object? DeserializeUsingCoreType(string toDeserialize, string coreType)
        {
            switch (coreType)
            {
                case TypeNames.Int:
                    return JsonSerializer.Deserialize<int>(toDeserialize);
                case TypeNames.Float:
                    return JsonSerializer.Deserialize<float>(toDeserialize);
                case TypeNames.Double:
                    return JsonSerializer.Deserialize<double>(toDeserialize);
                case TypeNames.Bool:
                    return JsonSerializer.Deserialize<bool>(toDeserialize);
                case TypeNames.String:
                    return JsonSerializer.Deserialize<string>(toDeserialize);
                case TypeNames.Color:
                    return JsonSerializer.Deserialize<Color>(toDeserialize);
                case TypeNames.Language:
                    return JsonSerializer.Deserialize<string>(toDeserialize);
                default:
                    return null;
            }
        }
    }
}
