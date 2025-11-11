using Avalonia.Input;
using Avalonia.Xaml.Interactions.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class KeyFilterTriggerBehavior : EventTriggerBehavior
    {
        public Key TargetKey { get; set; }

        protected override void OnEvent(object? args)
        {
            if (args is KeyEventArgs keyArgs && keyArgs.Key == TargetKey)
            {
                base.OnEvent(args);
                keyArgs.Handled = true;
            }
        }
    }
}
