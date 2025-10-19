using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Partlyx.UI.WPF.Helpers
{
    public class LocExtension : MarkupExtension, INotifyPropertyChanged
    {
        public string Key { get; set; }

        public LocExtension() { }
        public LocExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (App.LocService != null)
                App.LocService.CultureChanged += () => OnPropertyChanged(nameof(Value));

            return Value;
        }

        public string Value => App.LocService?.Get(Key) ?? "SEX";

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

}
