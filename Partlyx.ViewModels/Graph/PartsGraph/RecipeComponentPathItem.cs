using DynamicData;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Interfaces;
using Partlyx.ViewModels.UIStates;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.Graph.PartsGraph
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

        // Cached calculation result - single source of truth
        private PathCalculationResult? _cachedCalculationResult;
        public PathCalculationResult? CachedCalculationResult => _cachedCalculationResult;

        // Cached calculated data from CachedCalculationResult. Used for binding
        private ObservableCollection<ResourceAmountPairViewModel> _savedInputSums = new();
        public ReadOnlyObservableCollection<ResourceAmountPairViewModel> SavedInputSums { get; }
        private ObservableCollection<ResourceAmountPairViewModel> _savedOutputSums = new();
        public ReadOnlyObservableCollection<ResourceAmountPairViewModel> SavedOutputSums { get; }

        private bool _savedHasOutputs;
        public bool SavedHasOutputs { get => _savedHasOutputs; private set => SetProperty(ref _savedHasOutputs, value); }
        public int ComplexitySteps { get => _complexitySteps; private set => SetProperty(ref _complexitySteps, value); }

        // For path based graphs
        private double _savedEnterValue;
        public double SavedEnterValue { get => _savedEnterValue; private set => SetProperty(ref _savedEnterValue, value); }
        private bool _savedCalculateFromOutput;
        public bool SavedCalculateFromOutput { get => _savedCalculateFromOutput; private set => SetProperty(ref _savedCalculateFromOutput, value); }

        private int _complexitySteps;

        public double GetSavedSumFor(ResourceViewModel resource)
        {
            var pair = SavedInputSums.FirstOrDefault(p => p.Resource == resource) 
                    ?? SavedOutputSums.FirstOrDefault(p => p.Resource == resource);

            return pair == null ? default : pair.Amount;
        }
        public void UpdateSums(double amount, bool calculateFromOutput, bool adjustToArgument = true)
        {
            // Use CalculatePath as single source of truth
            var argumentType = calculateFromOutput 
                ? CalculationArgumentType.Output 
                : CalculationArgumentType.Input;
            var request = new CalculationRequest(amount, argumentType);
            
            _cachedCalculationResult = _path.CalculatePath(request, adjustToArgument);

            // Derive SavedInputSums and SavedOutputSums from cached result
            _savedInputSums.Clear();
            _savedOutputSums.Clear();

            var positiveSumsLookup = _cachedCalculationResult.ResourceTotals.ToLookup(s => s.Value > 0);

            var inputSums = positiveSumsLookup[false]
                .Select(kvp => new ResourceAmountPairViewModel(kvp.Key, -kvp.Value))
                .ToList();
            var outputSums = positiveSumsLookup[true]
                .Select(kvp => new ResourceAmountPairViewModel(kvp.Key, kvp.Value))
                .ToList();

            _savedInputSums.AddRange(inputSums);
            _savedOutputSums.AddRange(outputSums);

            SavedEnterValue = amount;
            SavedCalculateFromOutput = calculateFromOutput;

            SavedHasOutputs = _savedOutputSums.Any();
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
