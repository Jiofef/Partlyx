using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Partlyx.Core.Settings;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.Settings;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Partlyx.ViewModels.Settings
{
    public partial class SettingsServiceViewModel : ObservableObject
    {
        private readonly ISettingsService _service;
        public SettingsServiceViewModel(ISettingsService service)
        {
            _service = service;
        }

        private Dictionary<string, object?> _changedSettingsDic = new();
        public IReadOnlyDictionary<string, object?> ChangedSettingsDic => _changedSettingsDic;

        public ObservableCollection<OptionViewModel> ChangedSettings => new();

        private bool _isOptionsChanged;
        public bool IsOptionsChanged { get => _isOptionsChanged; set => SetProperty(ref _isOptionsChanged, value); }
        public List<string> GetChangedOptionsKeys() => _changedSettingsDic.Keys.ToList();
        public Dictionary<string, OptionViewModel?> GetOptionsDictionaryFromKeysList(IEnumerable<string> keys)
        {
            var dictionary = new Dictionary<string, OptionViewModel?>();

            foreach (var key in keys)
                dictionary.Add(key, UnsortedSettingsDictionary.GetValueOrDefault(key));

            return dictionary;
        }

        private SettingsGroupViewModel? _mainSettingsGroup;
        public SettingsGroupViewModel? MainSettingsGroup { get => _mainSettingsGroup; private set => SetProperty(ref _mainSettingsGroup, value); }


        private ObservableCollection<OptionViewModel> _unsortedSettings = new();
        public ObservableCollection<OptionViewModel> UnsortedSettings { get => _unsortedSettings; private set => SetProperty(ref _unsortedSettings, value); }


        private Dictionary<string, OptionViewModel> _unsortedSettingsDictionary = new();

        public Dictionary<string, OptionViewModel> UnsortedSettingsDictionary { get => _unsortedSettingsDictionary; private set => SetProperty(ref _unsortedSettingsDictionary, value); }
        private void OnSettingChanged(OptionViewModel setting)
        {
            _changedSettingsDic.Add(setting.Key, setting.Value);
            ChangedSettings.Add(setting);

            IsOptionsChanged = true;
        }
        public void ClearChangedSettings()
        {
            _changedSettingsDic.Clear();
            ChangedSettings.Clear();

            IsOptionsChanged = false;
        }
        public void BuildFromScheme(SettingsScheme scheme, bool addDefaultConverters = true)
        {
            foreach (var opt in UnsortedSettings)
            {
                var valueChangingAction = opt.ValueChanging;
                if (valueChangingAction != null)
                    valueChangingAction -= OnOptionValueChangedChanged;
            }

            MainSettingsGroup = new(scheme.OptionsGroup);

            UnsortedSettings = new(MainSettingsGroup.ToOneLevelOptionsList());
            UnsortedSettingsDictionary = UnsortedSettings.Select(s => new KeyValuePair<string, OptionViewModel>(s.Key, s)).ToDictionary();

            if (addDefaultConverters)
            {
                foreach (var setting in UnsortedSettings)
                {
                    if (setting is DecimalOptionViewModel decimalOption)
                        decimalOption.SettedValueConverter = new(value => value == null ? null : Convert.ToDecimal(value));
                    else if (setting is DoubleOptionViewModel doubleOption)
                        doubleOption.SettingValueConverter = new(value => value == null ? null : Convert.ToDouble(value));
                    else if (setting is FloatOptionViewModel floatOption)
                        floatOption.SettingValueConverter = new(value => value == null ? null : Convert.ToSingle(value));
                    else if (setting is IntOptionViewModel intOption)
                        intOption.SettingValueConverter = new(value => value == null ? null : Convert.ToInt32(value));
                }
            }

            ClearChangedSettings();

            foreach (var opt in UnsortedSettings)
            {
                opt.ValueChanging += OnOptionValueChangedChanged;
            }
        }
        public async Task<Dictionary<string, object?>> SyncValuesWithDB()
        {
            var options = await _service.GetAllOptionsAsync();
            var changedOptionValuesDic = new Dictionary<string, object?>();
            foreach (var option in options)
            {
                var optionVM = UnsortedSettingsDictionary.GetValueOrDefault(option.Key);
                if (optionVM == null || optionVM.Value == option.Value) continue;

                optionVM.SetValue(option.Value, true);
                changedOptionValuesDic.Add(optionVM.Key, optionVM.Value);
            }

            return changedOptionValuesDic;
        }
        public bool SetSetting(string key, object? value)
        {
            var setting = UnsortedSettingsDictionary[key];

            if (setting.Value != value)
            {
                setting.Value = value;
                return true;
            }
            return false;
        }
        public void OnOptionValueChangedChanged(OptionViewModel opt)
        {
            var key = opt.Key;
            if (!_changedSettingsDic.ContainsKey(key))
                OnSettingChanged(opt);
        }

        [RelayCommand]
        public async Task Apply()
        {
            foreach (var changedSettingKey in _changedSettingsDic.Keys)
            {
                var settingValue = UnsortedSettingsDictionary[changedSettingKey].GetConvertedValue();
                await _service.SetSettingValueAsync(changedSettingKey, settingValue);
            }
            ClearChangedSettings();
        }
        [RelayCommand]
        public void CancelChanges()
        {
            foreach (var settingKVPair in _changedSettingsDic)
                SetSetting(settingKVPair.Key, settingKVPair.Value);

            ClearChangedSettings();
        }
    }
}