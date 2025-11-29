// THIS FILE IS PART OF MODIFIED "Xaml.Behaviors.Avalonia" FOR PARTLYX

// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Input;
using Avalonia.Xaml.Interactions.DragAndDrop;

namespace Partlyx.UI.Avalonia.DragAndDrop;

/// <summary>
/// Behavior that starts a drag operation using the associated context data.
/// </summary>
public class ContextDragBehavior : ContextDragBehaviorBase
{
    /// <summary>
    /// Identifies the <see cref="Handler"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<IDragHandler?> HandlerProperty =
        AvaloniaProperty.Register<ContextDragBehavior, IDragHandler?>(nameof(Handler));

    /// <summary>
    /// Gets or sets the handler that receives drag notifications.
    /// </summary>
    public IDragHandler? Handler
    {
        get => GetValue(HandlerProperty);
        set => SetValue(HandlerProperty, value);
    }

    /// <inheritdoc />
    protected override void OnBeforeDragDrop(object? sender, PointerEventArgs e, object? context)
    {
        Handler?.BeforeDragDrop(sender, e, context);
    }

    /// <inheritdoc />
    protected override void OnAfterDragDrop(object? sender, PointerEventArgs e, object? context)
    {
        Handler?.AfterDragDrop(sender, e, context);
    }
}
