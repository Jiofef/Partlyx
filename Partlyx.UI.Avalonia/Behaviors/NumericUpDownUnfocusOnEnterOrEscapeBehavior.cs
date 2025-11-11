using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class NumericUpDownUnfocusOnEnterOrEscapeBehavior : Behavior<NumericUpDown>
    {
        private TextBox? _textBox;

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.TemplateApplied += AssociatedObject_TemplateApplied;
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.TemplateApplied -= AssociatedObject_TemplateApplied;
            }
            if (_textBox != null)
            {
                _textBox.KeyDown -= TextBox_KeyDown;
                _textBox = null;
            }
            base.OnDetaching();
        }

        private void AssociatedObject_TemplateApplied(object? sender, TemplateAppliedEventArgs e)
        {
            _textBox = e.NameScope.Find<TextBox>("PART_TextBox");

            if (_textBox != null)
            {
                _textBox.KeyDown += TextBox_KeyDown;
            }
        }

        private void TextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                AssociatedObject?.Focus();

                e.Handled = true;
            }
        }
    }
}
