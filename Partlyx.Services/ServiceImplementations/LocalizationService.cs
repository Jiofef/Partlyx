using Partlyx.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services.ServiceImplementations
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IApplicationResourceProvider _provider;
        private CultureInfo _culture;
        public Action CultureChanged { get; set; } = delegate { };
        public LocalizationService(IApplicationResourceProvider provider)
        {
            _provider = provider;
            _culture = CultureInfo.CurrentUICulture;
        }
        public string this[string key] => Get(key);
        public string Get(string key, params object[] args)
        {
            var format = _provider.GetString(key, _culture) ?? key;
            return args?.Length > 0 ? string.Format(_culture, format, args) : format;
        }
        public CultureInfo CurrentCulture => _culture;

        public void SetCulture(CultureInfo culture)
        {
            if (_culture.Equals(culture)) return;
            _culture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureChanged?.Invoke();
        }
    }
}
