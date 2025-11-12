using Avalonia;
using Avalonia.Input;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Partlyx.ViewModels.DragAndDrop.Interfaces;

namespace Partlyx.UI.Avalonia.DragAndDrop
{
    public class PartlyxContextDropBehavior : ContextDropBehavior
    {
        public static readonly StyledProperty<IDropHandlerViewModel?> DropHandlerProperty = AvaloniaProperty.Register<PartlyxContextDropBehavior, IDropHandlerViewModel?>("DropHandler");
        public IDropHandlerViewModel? DropHandler
        { 
            get => GetValue(DropHandlerProperty); 
            set => SetValue(DropHandlerProperty, value); 
        }

        public PartlyxContextDropBehavior()
        {
            Handler = new PartlyxDropHandlerDelegator(this);
        }
    }

    public class PartlyxDropHandlerDelegator : DropHandlerBase
    {
        private readonly PartlyxContextDropBehavior parentContext;
        public PartlyxDropHandlerDelegator(PartlyxContextDropBehavior context) 
        {
            parentContext = context;
        }
        public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
        {
            var dragOptionsVM = e.ToViewModel();
            bool result = parentContext.DropHandler?.Validate(sourceContext, dragOptionsVM) ?? false;

            e.DragEffects = dragOptionsVM.DragEffects.ToView();

            return result;
        }

        public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
            => parentContext.DropHandler?.Drop(sourceContext) ?? false;
    }
}
