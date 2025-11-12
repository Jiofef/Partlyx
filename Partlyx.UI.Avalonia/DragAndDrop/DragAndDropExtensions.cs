using Avalonia.Input;
using Partlyx.ViewModels.DragAndDrop;

namespace Partlyx.UI.Avalonia.DragAndDrop
{
    public static class DragAndDropExtensions
    {
        public static DragAndDropOptionsViewModel ToViewModel(this DragEventArgs e)
        {
            var dragEffects = e.DragEffects.ToViewModel();
            return new DragAndDropOptionsViewModel(dragEffects);
        }

        public static DragEffectsEnumViewModel ToViewModel(this DragDropEffects effects)
        {
            switch (effects)
            {
                case DragDropEffects.Copy:
                    return DragEffectsEnumViewModel.Copy;
                case DragDropEffects.Move:
                    return DragEffectsEnumViewModel.Move;
                case DragDropEffects.Link:
                    return DragEffectsEnumViewModel.Link;
                default:
                    return DragEffectsEnumViewModel.None;
            }

        }

        public static DragDropEffects ToView(this DragEffectsEnumViewModel effects)
        {
            switch (effects)
            {
                case DragEffectsEnumViewModel.Copy:
                    return DragDropEffects.Copy;
                case DragEffectsEnumViewModel.Move:
                    return DragDropEffects.Move;
                case DragEffectsEnumViewModel.Link:
                    return DragDropEffects.Link;
                default:
                    return DragDropEffects.None;
            }
        }
    }
}
