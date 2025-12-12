using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using Partlyx.Core.Technical;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.UIServices.Interfaces;
using ReactiveUI;
using SQLitePCL;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime;

namespace Partlyx.ViewModels.Settings
{
    public partial class ApplicationSettingsMenuViewModel : ObservableObject, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IDialogService _dialogService;
        private readonly IGlobalApplicationSettingsServiceViewModelContainer _settingsContainer;
        private readonly SettingsServiceViewModel _settings;

        private readonly List<IDisposable> _subscriptions = new();

        public SettingsGroupViewModel? MainSettingsGroup { get; private set; }

        private bool _isSettingsChanged;
        public bool IsSettingsChanged { get => _isSettingsChanged; private set => SetProperty(ref _isSettingsChanged, value); }
        public string DialogIdentifier { get; set; } = IDialogService.DefaultDialogIdentifier;

        public ApplicationSettingsMenuViewModel(IEventBus bus, IDialogService ds, IGlobalApplicationSettingsServiceViewModelContainer gassvmc)
        {
            _bus = bus;
            _dialogService = ds;

            _settingsContainer = gassvmc;
            _settings = _settingsContainer.SettingsService;

            MainSettingsGroup = _settings.MainSettingsGroup;

            var mainSettingsGroupChangedSubscription = _settings.WhenAnyValue(s => s.MainSettingsGroup).Subscribe(_ => MainSettingsGroup = _settings.MainSettingsGroup);
            _subscriptions.Add(mainSettingsGroupChangedSubscription);

            var settingsChangedChangedSubscription = _settings.WhenAnyValue(s => s.IsOptionsChanged).Subscribe(_ => IsSettingsChanged = _settings.IsOptionsChanged);
            _subscriptions.Add(settingsChangedChangedSubscription);
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        private void CloseDialog(object? arg)
        {
            _dialogService.Close(DialogIdentifier, arg);
        }

        [RelayCommand]
        public async Task Apply()
        {
            var changedKeys = _settings.GetChangedOptionsKeys();
            await _settings.Apply();
            var changedOptionsDictionary = _settings.GetOptionsDictionaryFromKeysList(changedKeys);
            var changedOptionValuesDictionary =
                 changedOptionsDictionary.Select(
                 kvp => new KeyValuePair<string, object?>(
                     kvp.Key,
                     kvp.Value?.Value))
                 .ToDictionary();
            _bus.Publish(new ApplicationSettingsAppliedViewModelEvent(changedOptionValuesDictionary, 
                new HashSet<object>(changedOptionsDictionary.Keys)));
        }
        [RelayCommand]
        public async Task Ok()
        {
            await _settings.Apply();
            CloseDialog(true);
        }
        [RelayCommand]
        public void Cancel()
        {
            _settings.CancelChanges();
            CloseDialog(false);
        }
    }

    public record ApplicationSettingsAppliedViewModelEvent(Dictionary<string, object?> ChangedSettingsDictionary, HashSet<object> ReceiverKeys) : IRoutedMultiKeyEvent;
}