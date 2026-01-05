namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    /// <summary> A simplified version of SelectedParts for elements that need one single element of one single type </summary>
    public interface IFocusedElementContainer
    {
        IFocusable? Focused { get; }
        FocusableElementTypeEnum? FocusedElementType { get; }

        bool HasFocusedElement { get; }
        void Focus(IFocusable? part);
    }
    public enum FocusableElementTypeEnum { RecipeHolder, ComponentPathHolder }

    public interface IIsolatedFocusedElementContainer : IFocusedElementContainer
    {

    }
    public interface IGlobalFocusedElementContainer : IFocusedElementContainer
    {

    }
}