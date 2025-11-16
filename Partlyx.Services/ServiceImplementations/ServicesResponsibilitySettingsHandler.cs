using Partlyx.Core.Contracts;
using Partlyx.Core.Settings;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.ServiceInterfaces;
using System.Globalization;
using System.Text.Json;

namespace Partlyx.Services.ServiceImplementations
{
    public class ServicesResponsibilitySettingsHandler : IServicesResponsibilitySettingsHandler, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ILocalizationService _localizationService;

        private readonly IDisposable _settingsChangedSubscription;
        private readonly IDisposable _settingsDBInitializedSubscription;

        private readonly Dictionary<string, Action<object?>> _responsibilityActions;

        public ServicesResponsibilitySettingsHandler(IEventBus bus, ISettingsRepository settings, ILocalizationService loc)
        {
            _bus = bus;
            _settingsRepository = settings;
            _localizationService = loc;

            _settingsChangedSubscription = _bus.Subscribe<SettingChangedEvent>(OnSettingChanged);
            _settingsDBInitializedSubscription = _bus.Subscribe<SettingsDBInitializedEvent>(OnSettingDBInitialized);

            _responsibilityActions = new()
            {
                {
                    SettingKeys.Language, new Action<object?>(value =>
                    {
                        var localeString = value as string;
                        if (localeString == null) return;

                        var locale = new CultureInfo(localeString);
                        _localizationService.SetCulture(locale);
                    })
                }
            };
        }

        private void OnSettingChanged(SettingChangedEvent ev)
        {
            if (!_responsibilityActions.ContainsKey(ev.OptionDto.Key)) return;

            var action = _responsibilityActions[ev.OptionDto.Key];
            action(ev.OptionDto.Value);
        }

        private void OnSettingDBInitialized(SettingsDBInitializedEvent ev)
        {
            Task.Run(async () =>
            {
                foreach (var kvp in _responsibilityActions)
                {
                    var valueFromDB = await _settingsRepository.GetDeserializedOptionValueStringAsync(kvp.Key);
                    var action = _responsibilityActions[kvp.Key];
                    action(valueFromDB);
                }
            });
        }

        public void Dispose()
        {
            _settingsChangedSubscription.Dispose();
            _settingsDBInitializedSubscription.Dispose();
        }
    }
}
