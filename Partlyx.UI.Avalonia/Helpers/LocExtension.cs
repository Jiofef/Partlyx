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
            var key = values.FirstOrDefault() as string ?? values.FirstOrDefault()?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            var localizedValue = App.LocService?.Get(key) ?? string.Empty;

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

        public IValueConverter? Converter { get; set; }
        public object? ConverterParameter { get; set; }

        public LocExtension() { }
        public LocExtension(string key) => Key = key;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (App.LocService != null)
                App.LocService.CultureChanged += OnCultureChanged;

            // IMPORTANT: Use Avalonia.Data.BindingBase to capture both standard and Compiled Bindings.
            if (Key is BindingBase keyBinding)
            {
                // CASE 2: Key is a dynamic binding (e.g., {Binding Name} or Key={Binding Name})

                var multiBinding = new MultiBinding
                {
                    // Use our specialized localization converter.
                    Converter = _bindingConverter,

                    // Pass the user's custom Converter and ConverterParameter to the LocBindingConverter 
                    // as a Tuple. LocBindingConverter will apply the localization first, then this user converter.
                    ConverterParameter = new Tuple<IValueConverter?, object?>(Converter, ConverterParameter),

                    Bindings =
            {
                // Binding 1: The key itself, coming from the ViewModel.
                keyBinding, 
                // Binding 2: The culture change trigger. When OnPropertyChanged(nameof(Value)) 
                // is called, this will force the MultiBinding to re-evaluate the key and re-localize.
                new Binding(nameof(Value)) { Source = this, Mode = BindingMode.OneWay }
            }
                };

                return multiBinding;
            }
            else
            {
                // CASE 1: Key is a static string (e.g., "Text" or Key="Text").
                // This relies on the Value property, which reads the static Key and gets the localized string.
                var b = new Binding(nameof(Value))
                {
                    Source = this,
                    Mode = BindingMode.OneWay,
                };

                // Apply the user's optional custom converter directly to this simple Binding.
                if (Converter != null)
                {
                    b.Converter = Converter;
                    b.ConverterParameter = ConverterParameter;
                }

                return b;
            }
        }

        private void OnCultureChanged()
        {
            OnPropertyChanged(nameof(Value));
        }

        public string Value
        {
            get
            {
                var keyStr = Key as string ?? Key?.ToString() ?? string.Empty;
                return App.LocService?.Get(keyStr) ?? string.Empty;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
