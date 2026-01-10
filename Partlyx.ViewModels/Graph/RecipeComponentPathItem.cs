using DynamicData;
using Partlyx.Core.Contracts;
using Partlyx.UI.Avalonia.Helpers;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Interfaces;
using Partlyx.ViewModels.UIStates;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.Graph
{
    public class RecipeComponentPathItem : PartlyxObservable, IFocusable
    {
        private readonly IComponentPathUiStateService _uiStateService;
        public RecipeComponentPathItem(RecipeComponentPath path, IComponentPathUiStateService uiStateService)
        {
            _path = path;
            _uiStateService = uiStateService;

            SavedInputSums = new(_savedInputSums);
            SavedOutputSums = new(_savedOutputSums);

            UpdateInfo();
        }
        // Path data
        private RecipeComponentPath _path;
        public RecipeComponentPath Path { get => _path; set { SetProperty(ref _path, value); UpdateInfo(); } }

        // Cached calculated data
        private ObservableCollection<ResourceAmountPairViewModel> _savedInputSums = new();
        public ReadOnlyObservableCollection<ResourceAmountPairViewModel> SavedInputSums { get; }
        private ObservableCollection<ResourceAmountPairViewModel> _savedOutputSums = new();
        public ReadOnlyObservableCollection<ResourceAmountPairViewModel> SavedOutputSums { get; }

        private int _complexitySteps;
        public int ComplexitySteps { get => _complexitySteps; private set => SetProperty(ref _complexitySteps, value); }

        public double GetSavedSumFor(ResourceViewModel resource)
        {
            var pair = SavedInputSums.FirstOrDefault(p => p.Resource == resource) 
                    ?? SavedOutputSums.FirstOrDefault(p => p.Resource == resource);

            return pair == null ? default : pair.Amount;
        }
        public void UpdateSums(double amount, bool calculateFromOutput)
        {
            var sumsUnsorted = calculateFromOutput 
                ? _path.QuantifyFromOutputAmountToValuePairs(amount)
                : _path.QuantifyToValuePairs(amount);

            _savedInputSums.Clear();
            _savedOutputSums.Clear();

            var positiveSumsLookup = sumsUnsorted.ToLookup(s => s.Amount > 0);

            var inputSums = positiveSumsLookup[false].ToList();
            var outputSums = positiveSumsLookup[true].ToList();
            // We invert all inputs, since it is clear from the context that these are spent resources
            foreach (var pair in inputSums)
                pair.Amount *= -1;

            _savedInputSums.AddRange(inputSums);
            _savedOutputSums.AddRange(outputSums);
        }
        public void UpdateInfo()
        {
            ComplexitySteps = Path.GetRecipesAmount();
        }
        
        public FocusableElementTypeEnum FocusableType => FocusableElementTypeEnum.ComponentPathHolder;
        public Guid Uid { get; } = Guid.NewGuid();

        public RecipeComponentPathItemUIState UiItem => _uiStateService.GetOrCreateItemUi(this);
        FocusableItemUIState IFocusable.UiItem => UiItem;
    }
}
