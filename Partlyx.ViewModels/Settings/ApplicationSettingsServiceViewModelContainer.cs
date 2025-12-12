using Partlyx.Core.Settings;
using Partlyx.Core.Technical;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.UIObjectViewModels;
using System.Reactive.Disposables;
using System.Runtime;

namespace Partlyx.ViewModels.Settings
{
    public class ApplicationSettingsServiceViewModelContainer : IGlobalApplicationSettingsServiceViewModelContainer, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly CompositeDisposable _disposables = new();
        public SettingsServiceViewModel SettingsService { get; }
        public ApplicationSettingsServiceViewModelContainer(SettingsServiceViewModel settings, IEventBus bus)
        {
            _bus = bus;
            var settingsDBInitializedSubscription = _bus.Subscribe<SettingsDBInitializedEvent>(OnSettingDBInitialized);
            _disposables.Add(settingsDBInitializedSubscription);


            SettingsService = settings;

            // Setting up the settings class
            SettingsService.BuildFromScheme(SettingsScheme.ApplicationSettings);
            
            // Setting up converters so services will get correct value to send in DB
            var languageSetting = SettingsService.UnsortedSettingsDictionary.GetValueOrDefault(SettingKeys.Language);
            if (languageSetting != null)
            {
                languageSetting.SettedValueConverter = new((unconvertedValue) => 
                {
                    if (unconvertedValue is LanguageInfo info)
                        return info.Code;

                    return unconvertedValue;
                });
                languageSetting.SettingValueConverter = new((unconvertedValue) =>
                {
                    if (unconvertedValue is string code)
                        return Languages.GetLanguage(code);

                    return unconvertedValue;
                });
            }
        }
        private void OnSettingDBInitialized(SettingsDBInitializedEvent ev)
        {
            Task.Run(async () =>
            {
                var changedOptionValuesDictionary = await SettingsService.SyncValuesWithDB();
                SettingsService.ClearChangedSettings();
                _bus.Publish(new ApplicationSettingsAppliedViewModelEvent(changedOptionValuesDictionary, 
                    new HashSet<object>(changedOptionValuesDictionary.Keys)));
            });
        }
        public void Dispose()
            => _disposables?.Dispose();
    }

    public interface IGlobalApplicationSettingsServiceViewModelContainer
    {
        SettingsServiceViewModel SettingsService { get; }
    }
}