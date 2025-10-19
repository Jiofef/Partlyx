using Partlyx.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Infrastructure.Data.ApplicationResources
{
    public class ApplicationResourcesProvider : IApplicationResourceProvider
    {
        private readonly Lazy<ResourceManager> _stringsResourceManager;
        public ApplicationResourcesProvider(Assembly resourcesAssembly)
        {
             _stringsResourceManager = new Lazy<ResourceManager>(() => 
                new ResourceManager("Partlyx.UI.WPF.Resources.Strings.Strings", resourcesAssembly),
                isThreadSafe: true);
        }
        public string GetString(string key, CultureInfo? culture = null)
            => _stringsResourceManager.Value.GetString(key, culture) ?? $"[{key}]";
    }
}
