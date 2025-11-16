using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Core.Settings;
using Partlyx.Core.Technical;
using Partlyx.Infrastructure.Events;
using System.Reactive.Disposables;

namespace Partlyx.ViewModels.Settings
{
    public partial class ApplicationSettingsProviderViewModel : ObservableObject, IDisposable
    {
        private IEventBus _bus;
        
        private CompositeDisposable _disposables = new();

        private Dictionary<string, Action<object?>> _settersDictionary;

        public ApplicationSettingsProviderViewModel(IEventBus bus)
        {
            _bus = bus;

            var applicationSettingChangedSubscription = _bus.Subscribe<ApplicationSettingsAppliedViewModelEvent>(OnSettingsApplied);
            _disposables.Add(applicationSettingChangedSubscription);

            var languageOptionScheme = SettingsScheme.ApplicationSettings.OptionsDictionary.GetValueOrDefault(SettingKeys.Language);
            var defaultLanguageKey = languageOptionScheme!.DefaultValueJson;
            var defaultLanguage = Languages.GetLanguage(defaultLanguageKey);
            _language = defaultLanguage!;

            _settersDictionary = new Dictionary<string, Action<object?>>()
            {
                { SettingKeys.Language, new((arg) => { Language = (LanguageInfo)arg!; }) },
            };
        }

        private void OnSettingsApplied(ApplicationSettingsAppliedViewModelEvent ev)
        {
            foreach (var kvp in _settersDictionary)
            {
                var action = _settersDictionary.GetValueOrDefault(kvp.Key);
                if (action != null)
                {
                    var changedOptionValue = kvp.Value;
                    action(changedOptionValue);
                }
            }
        }

        private LanguageInfo _language;
        public LanguageInfo Language { get => _language; private set => SetProperty(ref _language, value); }

        public void Dispose()
            => _disposables?.Dispose();
    }
}