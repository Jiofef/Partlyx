// THIS FILE IS PART OF MODIFIED "Xaml.Behaviors.Avalonia" FOR PARTLYX

// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Partlyx.UI.Avalonia.DragAndDrop;

namespace Partlyx.UI.Avalonia.DragAndDrop;

/// <summary>
/// Behavior that enables dropping context data onto the associated control using predefined <see cref="IDropHandler"/>.
/// </summary>
public class ContextDropBehavior : ContextDropBehaviorBase
{
    /// <summary>
    /// Identifies the <see cref="Handler"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<IDropHandler?> HandlerProperty =
        AvaloniaProperty.Register<ContextDropBehavior, IDropHandler?>(nameof(Handler));

    /// <summary>
    /// Gets or sets the drop handler that receives drop notifications.
    /// </summary>
    public IDropHandler? Handler
    {
        get => GetValue(HandlerProperty);
        set => SetValue(HandlerProperty, value);
    }

    /// <inheritdoc />
    protected override void OnEnter(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
    {
        Handler?.Enter(sender, e, sourceContext, targetContext);
    }

    /// <inheritdoc />
    protected override void OnLeave(object? sender, RoutedEventArgs e)
    {
        Handler?.Leave(sender, e);
    }

    /// <inheritdoc />
    protected override void OnOver(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
    {
        Handler?.Over(sender, e, sourceContext, targetContext);
    }

    /// <inheritdoc />
    protected override void OnDrop(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
    {
        Handler?.Drop(sender, e, sourceContext, targetContext);
    }
}
