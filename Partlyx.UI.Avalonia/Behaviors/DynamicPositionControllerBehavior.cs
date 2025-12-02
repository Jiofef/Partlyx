using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Partlyx.ViewModels.GraphicsViewModels;

namespace Partlyx.UI.Avalonia.Behaviors;

public class MovementInputBehavior : Behavior<InputElement>
{
    private bool _isUpPressed;
    private bool _isDownPressed;
    private bool _isLeftPressed;
    private bool _isRightPressed;

    private bool _isSpeedUpPressed;
    private bool _isSlowDownPressed;

    public static readonly StyledProperty<IDynamicPositionController?> ControllerProperty =
        AvaloniaProperty.Register<MovementInputBehavior, IDynamicPositionController?>(nameof(Controller));
    public IDynamicPositionController? Controller { get => GetValue(ControllerProperty); set => SetValue(ControllerProperty, value); }

    public static readonly StyledProperty<float> SpeedProperty =
        AvaloniaProperty.Register<MovementInputBehavior, float>(nameof(Speed), defaultValue: 1f);
    public float Speed { get => GetValue(SpeedProperty); set => SetValue(SpeedProperty, value); }

    public static readonly StyledProperty<Key> UpKeyProperty =
        AvaloniaProperty.Register<MovementInputBehavior, Key>(nameof(UpKey), defaultValue: Key.Up);
    public Key UpKey { get => GetValue(UpKeyProperty); set => SetValue(UpKeyProperty, value); }

    public static readonly StyledProperty<Key> DownKeyProperty =
        AvaloniaProperty.Register<MovementInputBehavior, Key>(nameof(DownKey), defaultValue: Key.Down);
    public Key DownKey { get => GetValue(DownKeyProperty); set => SetValue(DownKeyProperty, value); }

    public static readonly StyledProperty<Key> LeftKeyProperty =
        AvaloniaProperty.Register<MovementInputBehavior, Key>(nameof(LeftKey), defaultValue: Key.Left);
    public Key LeftKey { get => GetValue(LeftKeyProperty); set => SetValue(LeftKeyProperty, value); }

    public static readonly StyledProperty<Key> RightKeyProperty =
        AvaloniaProperty.Register<MovementInputBehavior, Key>(nameof(RightKey), defaultValue: Key.Right);
    public Key RightKey { get => GetValue(RightKeyProperty); set => SetValue(RightKeyProperty, value); }

    public static readonly StyledProperty<Key> SpeedUpKeyProperty =
        AvaloniaProperty.Register<MovementInputBehavior, Key>(nameof(SpeedUpKey), defaultValue: Key.LeftShift);
    public Key SpeedUpKey { get => GetValue(SpeedUpKeyProperty); set => SetValue(SpeedUpKeyProperty, value); }

    public static readonly StyledProperty<Key> SlowDownKeyProperty =
        AvaloniaProperty.Register<MovementInputBehavior, Key>(nameof(SlowDownKey), defaultValue: Key.LeftCtrl);
    public Key SlowDownKey { get => GetValue(SlowDownKeyProperty); set => SetValue(SlowDownKeyProperty, value); }

    public static readonly StyledProperty<float> SpeedUpMultiplierProperty =
        AvaloniaProperty.Register<MovementInputBehavior, float>(nameof(SpeedUpMultiplier), defaultValue: 2f);
    public float SpeedUpMultiplier { get => GetValue(SpeedUpMultiplierProperty); set => SetValue(SpeedUpMultiplierProperty, value); }

    public static readonly StyledProperty<float> SlowDownMultiplierProperty =
        AvaloniaProperty.Register<MovementInputBehavior, float>(nameof(SlowDownMultiplier), defaultValue: 0.5f);
    public float SlowDownMultiplier { get => GetValue(SlowDownMultiplierProperty); set => SetValue(SlowDownMultiplierProperty, value); }

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null)
        {
            AssociatedObject.KeyDown += OnKeyDown;
            AssociatedObject.KeyUp += OnKeyUp;
            AssociatedObject.LostFocus += OnLostFocus;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject != null)
        {
            AssociatedObject.KeyDown -= OnKeyDown;
            AssociatedObject.KeyUp -= OnKeyUp;
            AssociatedObject.LostFocus -= OnLostFocus;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (Controller == null) return;

        bool stateChanged = false;

        if (e.Key == UpKey) { _isUpPressed = true; stateChanged = true; }
        else if (e.Key == DownKey) { _isDownPressed = true; stateChanged = true; }
        else if (e.Key == LeftKey) { _isLeftPressed = true; stateChanged = true; }
        else if (e.Key == RightKey) { _isRightPressed = true; stateChanged = true; }

        if (e.Key == SpeedUpKey) { _isSpeedUpPressed = true; stateChanged = true; }
        else if (e.Key == SlowDownKey) { _isSlowDownPressed = true; stateChanged = true; }

        if (stateChanged)
        {
            UpdateVelocity();
        }
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (Controller == null) return;

        bool stateChanged = false;

        if (e.Key == UpKey) { _isUpPressed = false; stateChanged = true; }
        else if (e.Key == DownKey) { _isDownPressed = false; stateChanged = true; }
        else if (e.Key == LeftKey) { _isLeftPressed = false; stateChanged = true; }
        else if (e.Key == RightKey) { _isRightPressed = false; stateChanged = true; }

        if (e.Key == SpeedUpKey) { _isSpeedUpPressed = false; stateChanged = true; }
        else if (e.Key == SlowDownKey) { _isSlowDownPressed = false; stateChanged = true; }

        if (stateChanged)
        {
            UpdateVelocity();
        }
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        _isUpPressed = false;
        _isDownPressed = false;
        _isLeftPressed = false;
        _isRightPressed = false;
        _isSpeedUpPressed = false;
        _isSlowDownPressed = false;
        UpdateVelocity();
    }

    private void UpdateVelocity()
    {
        if (Controller == null) return;

        float currentMultiplier = 1f;

        if (_isSpeedUpPressed && !_isSlowDownPressed)
        {
            currentMultiplier = SpeedUpMultiplier;
        }
        else if (_isSlowDownPressed && !_isSpeedUpPressed)
        {
            currentMultiplier = SlowDownMultiplier;
        }

        float velX = 0;
        if (_isLeftPressed) velX -= Speed;
        if (_isRightPressed) velX += Speed;

        float velY = 0;
        if (_isUpPressed) velY -= Speed;
        if (_isDownPressed) velY += Speed;

        Controller.VelocityX = velX * currentMultiplier;
        Controller.VelocityY = velY * currentMultiplier;
    }
}