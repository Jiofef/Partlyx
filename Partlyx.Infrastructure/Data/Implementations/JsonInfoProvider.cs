using Newtonsoft.Json;
using Partlyx.Core.Contracts;
using Partlyx.Core.OtherSaves;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using System.Reflection.Metadata.Ecma335;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public class JsonInfoProvider : IJsonInfoProvider
    {
        private readonly IEventBus _bus;
        private readonly IJsonLoader _loader;
        private readonly IJsonSaver _saver;

        private readonly Dictionary<string, Dictionary<string, object?>> _saves = new();
        private readonly Dictionary<string, SaveScheme> _saveSchemes = new();
        private readonly List<SaveScheme> _loadedSchemes = new();

        private JsonSerializerSettings _defaultSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented };
        public JsonInfoProvider(IEventBus bus, IJsonLoader loader, IJsonSaver saver)
        {
            _bus = bus;
            _loader = loader;
            _saver = saver;
        }

        public bool IsSchemeLoadingFinished(SaveScheme scheme)
            => _loadedSchemes.Contains(scheme);

        public async Task<bool> WaitUntilSchemeIsLoaded(SaveScheme scheme, double maxWaitTimeSec = 5.0)
        {
            if (IsSchemeLoadingFinished(scheme))
                return true;

            var tcs = new TaskCompletionSource<bool>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(maxWaitTimeSec));

            void OnSchemeLoaded(JsonInfoLoadingFinishedEvent ev)
            {
                if (ev.JsonSchemeName == scheme.SchemeName)
                {
                    tcs.SetResult(true);
                }
            }

            var subscription = _bus.Subscribe<JsonInfoLoadingFinishedEvent>(OnSchemeLoaded);

            try
            {
                using (cts.Token.Register(() => tcs.SetResult(false)))
                {
                    return await tcs.Task;
                }
            }
            finally
            {
                subscription.Dispose();
            }
        }

        public T? GetValue<T>(string saveKey, string valueKey)
        {
            var value = _saves.GetValueOrDefault(saveKey)?.GetValueOrDefault(valueKey);
            if (value is T tValue)
                return tValue;
            return default;
        }
        public object? GetValue(string saveKey, string valueKey)
        {
            return _saves.GetValueOrDefault(saveKey)?.GetValueOrDefault(valueKey);
        }

        public bool SetValue(string saveKey, string valueKey, object? value)
        {
            if (_saves.ContainsKey(saveKey) && _saves[saveKey].ContainsKey(valueKey))
            {
                _saves[saveKey][valueKey] = value;
                return true;
            }
            return false;
        }

        public async Task<bool> SetValueAndSaveAsync(string saveKey, string valueKey, object? value)
        {
            var setResult = SetValue(saveKey, valueKey, value);

            if (!setResult)
                return false;

            return await TrySaveSchemeAsync(saveKey);
        }

        public async Task LoadGlobalSchemesAsync()
        {
            foreach (var scheme in SaveScheme.GlobalSchemes)
            {
                await TryLoadSchemeAsync(scheme);
            }
        }

        public async Task<bool> TrySaveSchemeAsync(string schemeName)
        {
            if (_saveSchemes.ContainsKey(schemeName))
            {
                var scheme = _saveSchemes[schemeName];
                return await TrySaveSchemeAsync(scheme);
            }
            return false;
        }

        public async Task<bool> TrySaveSchemeAsync(SaveScheme scheme)
        {
            var path = Path.Combine(DirectoryManager.PartlyxDataDirectory, scheme.SchemeName + ".json");

            var saveState = _saves.GetValueOrDefault(scheme.SchemeName);
            if (saveState == null)
                return false;

            var result = await _saver.TrySaveAsync(path, JsonConvert.SerializeObject(saveState, _defaultSettings));
            return result;
        }

        public async Task<bool> TryLoadSchemeAsync(SaveScheme scheme)
        {
            var dic = scheme.GetAsObjectsDictionary();
            _saves.Add(scheme.SchemeName, dic);
            _saveSchemes.Add(scheme.SchemeName, scheme);

            var path = Path.Combine(DirectoryManager.PartlyxDataDirectory, scheme.SchemeName + ".json");
            var json = await _loader.TryLoadAsync(path);
            if (json == null)
            {
                OnSchemeLoaded(false);
                return false;
            }

            var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, _defaultSettings);
            if (model == null)
            {
                OnSchemeLoaded(false);
                return false;
            }
            foreach (var key in dic.Keys)
            {
                var schematicProp = scheme.SavePropertiesDic[key];
                var value = model.GetValueOrDefault(key);

                if (value == null && !schematicProp.AllowNull)
                    continue;
                dic[key] = value;
            }
            OnSchemeLoaded(true);

            void OnSchemeLoaded(bool success)
            {
                try
                {
                    _bus.Publish(new JsonInfoLoadingFinishedEvent(scheme.SchemeName, path, success));
                }
                catch { }
                _loadedSchemes.Add(scheme);
            }

            return true;
        }
    }

    public record JsonInfoLoadingFinishedEvent(string JsonSchemeName, string FilePath, bool Success);
}
