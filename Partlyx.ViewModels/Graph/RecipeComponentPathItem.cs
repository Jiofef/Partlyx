using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Interfaces;
using Partlyx.ViewModels.UIStates;

namespace Partlyx.ViewModels.Graph
{
    public class RecipeComponentPathItem : PartlyxObservable, IFocusable
    {
        private readonly IComponentPathUiStateService _uiStateService;

        public RecipeComponentPathItem(IComponentPathUiStateService uiStateService)
        {
            _uiStateService = uiStateService;
        }

        private RecipeComponentPath _path;
        public RecipeComponentPath Path { get => _path; set => SetProperty(ref _path, value); }
        public FocusableElementTypeEnum FocusableType => FocusableElementTypeEnum.ComponentPathHolder;
        public Guid Uid { get; } = Guid.NewGuid();

        public RecipeComponentPathItemUIState UiItem => _uiStateService.GetOrCreateItemUi(this);
        FocusableItemUIState IFocusable.UiItem => UiItem;
    }
}
