using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Globalization;
using static Avalonia.Media.TextTrimming;

namespace Partlyx.UI.Avalonia.OtherControls
{
    public class FitTextBlock : TextBlock
    {
        private double _maxFontSize;

        public FitTextBlock()
        {
            TextTrimming = CharacterEllipsis;
        }

        public static readonly StyledProperty<double> MinFontSizeProperty =
            AvaloniaProperty.Register<FitTextBlock, double>(
                nameof(MinFontSize),
                defaultValue: 5.0);

        public double MinFontSize
        {
            get => GetValue(MinFontSizeProperty);
            set => SetValue(MinFontSizeProperty, value);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _maxFontSize = FontSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (string.IsNullOrEmpty(Text))
            {
                TextTrimming = CharacterEllipsis;
                return base.MeasureOverride(availableSize);
            }

            double currentMax = _maxFontSize;

            if (double.IsInfinity(availableSize.Width))
            {
                if (Math.Abs(FontSize - currentMax) > 0.01) FontSize = currentMax;
                TextTrimming = CharacterEllipsis;
                return base.MeasureOverride(availableSize);
            }

            var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

            var formattedText = new FormattedText(
                Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                currentMax,
                Foreground
            );

            bool useEllipsisFallback = false;
            double newSize = currentMax;

            if (formattedText.Width > availableSize.Width)
            {
                double ratio = availableSize.Width / formattedText.Width;
                newSize = currentMax * ratio;

                newSize = Math.Max(newSize, MinFontSize);

                if (Math.Abs(newSize - MinFontSize) < 0.1)
                {
                    var minSizeText = new FormattedText(
                        Text,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        MinFontSize,
                        Foreground
                    );

                    if (minSizeText.Width > availableSize.Width)
                    {
                        useEllipsisFallback = true;
                    }
                }
            }

            if (Math.Abs(FontSize - newSize) > 0.1)
            {
                FontSize = newSize;
            }
            else if (newSize == currentMax && Math.Abs(FontSize - currentMax) > 0.1)
            {
                FontSize = currentMax;
            }

            TextTrimming = useEllipsisFallback ? CharacterEllipsis : TextTrimming.None;

            return base.MeasureOverride(availableSize);
        }
    }
}