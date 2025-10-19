using System.Globalization;

namespace Partlyx.Core.Contracts
{
    public interface ILocalizationService
    {
        string this[string key] { get; }
        string Get(string key, params object[] args);
        CultureInfo CurrentCulture { get; }
        Action CultureChanged { get; set; }
        void SetCulture(CultureInfo culture);
    }
}
