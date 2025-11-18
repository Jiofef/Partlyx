using Partlyx.Core.OtherSaves;

namespace Partlyx.Infrastructure.Data.Interfaces
{
    public interface IJsonInfoProvider
    {
        object? GetValue(string saveKey, string valueKey);
        T? GetValue<T>(string saveKey, string valueKey);
        Task LoadGlobalSchemesAsync();
        bool SetValue(string saveKey, string valueKey, object? value);
        Task<bool> SetValueAndSaveAsync(string saveKey, string valueKey, object? value);
        Task<bool> TryLoadSchemeAsync(SaveScheme scheme);
        Task<bool> TrySaveSchemeAsync(SaveScheme scheme);
        Task<bool> TrySaveSchemeAsync(string schemeName);
        Task<bool> WaitUntilSchemeIsLoaded(SaveScheme scheme, double maxWaitTimeSec = 5.0);
    }
}