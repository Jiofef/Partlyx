using Partlyx.Core.OtherSaves;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public class JsonSavesService : IJsonSavesService
    {
        private IJsonInfoProvider _provider;
        public JsonSavesService(IJsonInfoProvider prov)
        {
            _provider = prov;
        }

        public T? GetValue<T>(string saveKey, string valueKey)
            => _provider.GetValue<T>(saveKey, valueKey);
        public object? GetValue(string saveKey, string valueKey)
            => _provider.GetValue(saveKey, valueKey);

        public Task LoadGlobalSchemesAsync()
            => _provider.LoadGlobalSchemesAsync();

        public Task<bool> WaitUntilSchemeIsLoaded(SaveScheme scheme, double maxWaitTimeSec = 5.0)
            => _provider.WaitUntilSchemeIsLoaded(scheme, maxWaitTimeSec);
        public bool SetValue(string saveKey, string valueKey, object? value)
            => _provider.SetValue(saveKey, valueKey, value);

        public Task<bool> SetValueAndSaveAsync(string saveKey, string valueKey, object? value)
            => _provider.SetValueAndSaveAsync(saveKey, valueKey, value);

        public Task<bool> TryLoadSchemeAsync(SaveScheme scheme)
            => _provider.TryLoadSchemeAsync(scheme);

        public Task<bool> TrySaveSchemeAsync(SaveScheme scheme)
            => _provider.TrySaveSchemeAsync(scheme);

        public Task<bool> TrySaveSchemeAsync(string schemeName)
            => _provider.TrySaveSchemeAsync(schemeName);

    }
}
