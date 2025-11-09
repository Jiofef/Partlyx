using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;

namespace Partlyx.UI.Avalonia.OtherControls
{
    public class ZoomPanControl : ContentControl
    {
        public static readonly StyledProperty<double> ZoomLevelProperty =
            AvaloniaProperty.Register<ZoomPanControl, double>(
                nameof(ZoomLevel),
                1.0,
                coerce: CoerceZoomLevel);

        public static readonly StyledProperty<double> PanPositionXProperty =
            AvaloniaProperty.Register<ZoomPanControl, double>(
                nameof(PanPositionX),
                0.0);

        public static readonly StyledProperty<double> PanPositionYProperty =
            AvaloniaProperty.Register<ZoomPanControl, double>(
                nameof(PanPositionY),
                0.0);

        public static readonly StyledProperty<double> ZoomSpeedProperty =
            AvaloniaProperty.Register<ZoomPanControl, double>(
                nameof(ZoomSpeed),
                1.2);

        public static readonly StyledProperty<double> MinZoomProperty =
            AvaloniaProperty.Register<ZoomPanControl, double>(
                nameof(MinZoom),
                0.1);

        public static readonly StyledProperty<double> MaxZoomProperty =
            AvaloniaProperty.Register<ZoomPanControl, double>(
                nameof(MaxZoom),
                10.0);

        private ScaleTransform _scaleTransform;
        private TranslateTransform _translateTransform;
        private Point _lastMousePos;
        private bool _isPanning = false;

        public double ZoomLevel
        {
            get => GetValue(ZoomLevelProperty);
            set => SetValue(ZoomLevelProperty, value);
        }

        public double PanPositionX
        {
            get => GetValue(PanPositionXProperty);
            set => SetValue(PanPositionXProperty, value);
        }

        public double PanPositionY
        {
            get => GetValue(PanPositionYProperty);
            set => SetValue(PanPositionYProperty, value);
        }

        public double ZoomSpeed
        {
            get => GetValue(ZoomSpeedProperty);
            set => SetValue(ZoomSpeedProperty, value);
        }

        public double MinZoom
        {
            get => GetValue(MinZoomProperty);
            set => SetValue(MinZoomProperty, value);
        }

        public double MaxZoom
        {
            get => GetValue(MaxZoomProperty);
            set => SetValue(MaxZoomProperty, value);
        }

        static ZoomPanControl()
        {
            ZoomLevelProperty.Changed.AddClassHandler<ZoomPanControl>((x, e) => x.OnZoomLevelChanged(e));
            PanPositionXProperty.Changed.AddClassHandler<ZoomPanControl>((x, e) => x.OnPanPositionChanged(e));
            PanPositionYProperty.Changed.AddClassHandler<ZoomPanControl>((x, e) => x.OnPanPositionChanged(e));
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            var contentPresenter = e.NameScope.Find<Control>("ContentPresenter");
            if (contentPresenter != null)
            {
                var transformGroup = new TransformGroup();
                _scaleTransform = new ScaleTransform();
                _translateTransform = new TranslateTransform();
                transformGroup.Children.Add(_scaleTransform);
                transformGroup.Children.Add(_translateTransform);
                contentPresenter.RenderTransform = transformGroup;
                contentPresenter.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative);

                UpdateTransforms();
            }
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            if (e.KeyModifiers != KeyModifiers.None) return;

            double zoomFactor = e.Delta.Y > 0 ? ZoomSpeed : 1 / ZoomSpeed;
            double oldZoom = ZoomLevel;
            double newZoom = Math.Clamp(oldZoom * zoomFactor, MinZoom, MaxZoom);

            var pos = e.GetPosition(this);

            PanPositionX = pos.X - (pos.X - PanPositionX) * (newZoom / oldZoom);
            PanPositionY = pos.Y - (pos.Y - PanPositionY) * (newZoom / oldZoom);

            ZoomLevel = newZoom;
            e.Handled = true;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _isPanning = true;
                _lastMousePos = e.GetPosition(this);
                Cursor = new Cursor(StandardCursorType.Hand);
                e.Handled = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                _isPanning = false;
                Cursor = new Cursor(StandardCursorType.Arrow);
                e.Handled = true;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (_isPanning && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                var mousePos = e.GetPosition(this);
                var mouseDelta = new Point(mousePos.X - _lastMousePos.X, mousePos.Y - _lastMousePos.Y);

                PanPositionX += mouseDelta.X;
                PanPositionY += mouseDelta.Y;

                _lastMousePos = mousePos;
                e.Handled = true;
            }
        }

        private static double CoerceZoomLevel(AvaloniaObject d, double value)
        {
            var control = (ZoomPanControl)d;
            return Math.Clamp(value, control.MinZoom, control.MaxZoom);
        }

        private void OnZoomLevelChanged(AvaloniaPropertyChangedEventArgs e)
        {
            UpdateTransforms();
        }

        private void OnPanPositionChanged(AvaloniaPropertyChangedEventArgs e)
        {
            UpdateTransforms();
        }

        private void UpdateTransforms()
        {
            if (_scaleTransform != null)
            {
                _scaleTransform.ScaleX = ZoomLevel;
                _scaleTransform.ScaleY = ZoomLevel;
            }
            if (_translateTransform != null)
            {
                _translateTransform.X = PanPositionX;
                _translateTransform.Y = PanPositionY;
            }
        }
    }
}