using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Partlyx.UI.WPF.OtherControls;

public class ZoomPanControl : ContentControl
{
    public static readonly DependencyProperty ZoomLevelProperty =
        DependencyProperty.Register("ZoomLevel", typeof(double), typeof(ZoomPanControl), new PropertyMetadata(1.0, OnZoomLevelChanged));

    public static readonly DependencyProperty PanPositionXProperty =
        DependencyProperty.Register("PanPositionX", typeof(double), typeof(ZoomPanControl), new PropertyMetadata(0.0, OnPanPositionChanged));
    public static readonly DependencyProperty PanPositionYProperty =
        DependencyProperty.Register("PanPositionY", typeof(double), typeof(ZoomPanControl), new PropertyMetadata(0.0, OnPanPositionChanged));

    public static readonly DependencyProperty ZoomSpeedProperty =
        DependencyProperty.Register("ZoomSpeed", typeof(double), typeof(ZoomPanControl), new PropertyMetadata(1.2));

    public static readonly DependencyProperty MinZoomProperty =
        DependencyProperty.Register("MinZoom", typeof(double), typeof(ZoomPanControl), new PropertyMetadata(0.1));

    public static readonly DependencyProperty MaxZoomProperty =
        DependencyProperty.Register("MaxZoom", typeof(double), typeof(ZoomPanControl), new PropertyMetadata(10.0));

    public double ZoomLevel
    {
        get => (double)GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, Math.Clamp(value, MinZoom, MaxZoom));
    }

    public double PanPositionX
    {
        get => (double)GetValue(PanPositionXProperty);
        set => SetValue(PanPositionXProperty, value);
    }
    public double PanPositionY
    {
        get => (double)GetValue(PanPositionYProperty);
        set => SetValue(PanPositionYProperty, value);
    }

    public double ZoomSpeed
    {
        get => (double)GetValue(ZoomSpeedProperty);
        set => SetValue(ZoomSpeedProperty, value);
    }

    public double MinZoom
    {
        get => (double)GetValue(MinZoomProperty);
        set => SetValue(MinZoomProperty, value);
    }

    public double MaxZoom
    {
        get => (double)GetValue(MaxZoomProperty);
        set => SetValue(MaxZoomProperty, value);
    }

    private ScaleTransform _scaleTransform;
    private TranslateTransform _translateTransform;

    static ZoomPanControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomPanControl), new FrameworkPropertyMetadata(typeof(ZoomPanControl)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        var contentPresenter = GetTemplateChild("ContentPresenter") as FrameworkElement;
        if (contentPresenter != null)
        {
            var transformGroup = new TransformGroup();
            _scaleTransform = new ScaleTransform();
            _translateTransform = new TranslateTransform();
            transformGroup.Children.Add(_scaleTransform);
            transformGroup.Children.Add(_translateTransform);
            contentPresenter.RenderTransform = transformGroup;
            contentPresenter.RenderTransformOrigin = new Point(0, 0);

            PreviewMouseWheel += OnMouseWheel;
            PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            PreviewMouseMove += OnMouseMove;

            UpdateTransforms();
        }
    }

    private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ZoomPanControl)d;
        control.UpdateTransforms();
    }

    private static void OnPanPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ZoomPanControl)d;
        control.UpdateTransforms();
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
            Debug.WriteLine(PanPositionX + " " + PanPositionY);
            _translateTransform.X = PanPositionX;
            _translateTransform.Y = PanPositionY;
        }
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.None) return;

        double zoomFactor = e.Delta > 0 ? ZoomSpeed : 1 / ZoomSpeed;
        double oldZoom = ZoomLevel;
        double newZoom = Math.Clamp(oldZoom * zoomFactor, MinZoom, MaxZoom);

        var pos = e.GetPosition(this);

        PanPositionX = pos.X - (pos.X - PanPositionX) * (newZoom / oldZoom);
        PanPositionY = pos.Y - (pos.Y - PanPositionY) * (newZoom / oldZoom);

        ZoomLevel = newZoom;
        e.Handled = true;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        CaptureMouse();
    }

    private void StartPanning(MouseEventArgs e)
    {
        _isPanning = true;
        _lastMousePos = e.GetPosition(this);
        Cursor = Cursors.Hand;
        e.Handled = true;
    }
    private void StopPanning()
    {
        _isPanning = false;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ReleaseMouseCapture();
        Cursor = Cursors.Arrow;
        e.Handled = true;
    }

    private Point _lastMousePos;
    private bool _isPanning = false;
    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (IsMouseCaptured)
        {
            if (!_isPanning)
                StartPanning(e);
            var mousePos = e.GetPosition(this);
            var mouseDelta = new Point(mousePos.X - _lastMousePos.X, mousePos.Y - _lastMousePos.Y);

            PanPositionX += mouseDelta.X;
            PanPositionY += mouseDelta.Y;

            _lastMousePos = mousePos;

            e.Handled = true;
        }
        else
            StopPanning();
    }
}