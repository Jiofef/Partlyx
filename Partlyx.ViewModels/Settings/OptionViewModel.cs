using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Core.Settings;
using Partlyx.Core.Technical;

namespace Partlyx.ViewModels.Settings
{
    public abstract class OptionViewModel : ObservableObject
    {
        private SchematicOption _baseSheme;

        public OptionViewModel(SchematicOption scheme)
        {
            _baseSheme = scheme;

            Key = scheme.Key;
            Name = scheme.Name;

            SettedValueConverter = new((argValue) => { return argValue; });
            SettingValueConverter = new((argValue) => { return argValue; });
        }
        public static OptionViewModel Create(SchematicOption scheme)
        {
            switch(scheme.TypeName)
            {
                case TypeNames.Int:
                    return new IntOptionViewModel(scheme);
                case TypeNames.Float:
                    return new FloatOptionViewModel(scheme);
                case TypeNames.Double:
                    return new DoubleOptionViewModel(scheme);
                case TypeNames.Bool:
                    return new BoolOptionViewModel(scheme);
                case TypeNames.String:
                    return new StringOptionViewModel(scheme);
                case TypeNames.Color:
                    return new ColorOptionViewModel(scheme);
                case TypeNames.Language:
                    return new LanguageOptionViewModel(scheme);
                default: 
                    return new StringOptionViewModel(scheme);
            }
        }

        public bool AllowNull = false;
        public string Key { get; }
        public string Name { get; }

        private object? _value;
        public object? Value { get => _value; set => SetValue(value); }
        public void SetValue(object? value)
        {
            bool equals =
                _value is double doubleValue && Value is double currentDoubleValue && Math.Abs(doubleValue - currentDoubleValue) < 1e-10
                || _value is float floatValue && Value is float currentFloatValue && Math.Abs(floatValue - currentFloatValue) < 1e-6f
                || EqualityComparer<object>.Default.Equals(Value, value);

            if (!equals && (value != null || AllowNull))
            {
                ValueChanging?.Invoke(this);
                SetProperty(ref _value, value, nameof(Value));
            }
        }
        public Action<OptionViewModel> ValueChanging = delegate { };

        public object? GetConvertedValue() => SettedValueConverter(Value);
        public void SetValueAndConvert(object? value)
        {
            var convertedValue = SettingValueConverter(value);
            Value = convertedValue;
        }

        public Func<object?, object?> SettedValueConverter; // use when "this -> settings service"
        public Func<object?, object?> SettingValueConverter; // use when "settings service -> this"

        public OptionViewModel Duplicate() 
        {
            var duplicate = Create(_baseSheme);
            duplicate.Value = Value;
            return duplicate;
        }
    }

    public class IntOptionViewModel : OptionViewModel
    {
        public IntOptionViewModel(SchematicOption scheme) : base (scheme) { }
    }
    public class FloatOptionViewModel : OptionViewModel
    {
        public FloatOptionViewModel(SchematicOption scheme) : base(scheme) { }
    }
    public class DoubleOptionViewModel : OptionViewModel
    {
        public DoubleOptionViewModel(SchematicOption scheme) : base(scheme) { }
    }
    public class BoolOptionViewModel : OptionViewModel
    {
        public BoolOptionViewModel(SchematicOption scheme) : base(scheme) { }
    }
    public class StringOptionViewModel : OptionViewModel
    {
        public StringOptionViewModel(SchematicOption scheme) : base(scheme) { }
    }
    public class ColorOptionViewModel : OptionViewModel
    {
        public ColorOptionViewModel(SchematicOption scheme) : base(scheme) { }
    }
    public class LanguageOptionViewModel : OptionViewModel
    {
        public LanguageOptionViewModel(SchematicOption scheme) : base(scheme) 
        {
            AvailableLanguages = Languages.AvailableLanguagesList;
        }

        public IReadOnlyList<LanguageInfo> AvailableLanguages { get; }
    }
}