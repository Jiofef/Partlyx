using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Partlyx.UI.Avalonia.Helpers
{
    public class LocBindingConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Count == 0) return string.Empty;

            var key = values[0] as string ?? values[0]?.ToString() ?? string.Empty;
            var args = values.Skip(1).Take(values.Count - 2).ToArray();

            if (string.IsNullOrEmpty(key)) return string.Empty;

            var localizedValue = App.LocService?.Get(key, args) ?? string.Empty;

            if (parameter is Tuple<IValueConverter?, object?> userConverterTuple)
            {
                var userConverter = userConverterTuple.Item1;
                var userParam = userConverterTuple.Item2;

                if (userConverter != null)
                {
                    return userConverter.Convert(localizedValue, targetType, userParam, culture);
                }
            }

            return localizedValue;
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class LocExtension : MarkupExtension, INotifyPropertyChanged
    {
        private static readonly LocBindingConverter _bindingConverter = new LocBindingConverter();

        public object? Key { get; set; }

        [AssignBinding]
        public List<BindingBase> Arguments { get; set; } = new List<BindingBase>();

        public BindingBase Argument
        {
            set => Arguments.Add(value);
        }

        public IValueConverter? Converter { get; set; }
        public object? ConverterParameter { get; set; }

        public LocExtension() { }
        public LocExtension(string key) => Key = key;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (App.LocService != null)
                App.LocService.CultureChanged += OnCultureChanged;

            var multiBinding = new MultiBinding
            {
                Converter = _bindingConverter,
                ConverterParameter = new Tuple<IValueConverter?, object?>(Converter, ConverterParameter)
            };

            if (Key is BindingBase keyBinding)
            {
                multiBinding.Bindings.Add(keyBinding);
            }
            else
            {
                multiBinding.Bindings.Add(new Binding { Source = Key?.ToString() ?? string.Empty, Mode = BindingMode.OneTime });
            }

            foreach (var argBinding in Arguments)
            {
                multiBinding.Bindings.Add(argBinding);
            }

            multiBinding.Bindings.Add(new Binding(nameof(Value)) { Source = this, Mode = BindingMode.OneWay });

            return multiBinding;
        }

        private void OnCultureChanged() => OnPropertyChanged(nameof(Value));

        public string Value => string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}