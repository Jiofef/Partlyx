using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.Settings;
using Partlyx.ViewModels.UIServices;
using ReactiveUI;
using UJL.CSharp.Collections;

namespace Partlyx.ViewModels.Graph.PartsGraph
{
    public class ComponentGraphNodeViewModel : GraphNodeViewModel, IDisposable, ITypedVMPartHolder<RecipeComponentViewModel>
    {
        private readonly ApplicationSettingsProviderViewModel _settingsProvider;

        private readonly IDisposable _resourceNameUpdateSubscription;
        private readonly IDisposable _valueUpdateSubscription;
        private readonly IDisposable _decimalPlacesChangedSubscription;

        private string _columnTextPart1 = "";
        private string _columnTextPart2 = "";
        public string ColumnText1 { get => _columnTextPart1; private set => SetProperty(ref _columnTextPart1, value); }
        public string ColumnText2 { get => _columnTextPart2; private set => SetProperty(ref _columnTextPart2, value); }

        public ComponentGraphNodeViewModel(RecipeComponentViewModel value, GraphNodeViewModel? mainRelative = null)
            : base(mainRelative, value)
        {
            var component = (RecipeComponentViewModel)Value;
            _component = component;
            IsOutput = component.IsOutput;

            _settingsProvider = component.GlobalInfo.ApplicationSettings;

            _resourceNameUpdateSubscription =
                this.WhenAnyValue((@this => @this._component.LinkedResource.Value.Name))
                .Subscribe((o) => UpdateColumnText());

            _valueUpdateSubscription = 
                this.WhenAnyValue(@this => @this.Value)
                .Subscribe((o) => OnValueChanged());

            _decimalPlacesChangedSubscription = _settingsProvider
                .WhenAnyValue(p => p.DecimalPlacesInGraphNodes)
                .Subscribe(_ => UpdateColumnText());
        }

        private void OnValueChanged()
        {
            if (Value is RecipeComponentViewModel component)
            {
                Part = component;
            }
            UpdateColumnText();
        }
        private double _absCost;
        public double AbsCost { get => _absCost; set 
            {
                if (SetProperty(ref _absCost, value))
                {
                    OnPropertyChanged(nameof(Cost));
                    UpdateColumnText(); 
                }
            }
        }

        private bool _isOutput;
        public bool IsOutput { get => _isOutput;
            set
            {
                if (SetProperty(ref _isOutput, value))
                    OnPropertyChanged(nameof(Cost));
            }
        }

        public double Cost { get => IsOutput ? -AbsCost : AbsCost; }

        public PartTypeEnumVM? PartType => PartTypeEnumVM.Component;
        private RecipeComponentViewModel? _component = null;

        public RecipeComponentViewModel? Part { get => _component; private set => SetProperty(ref _component, value); }

        private void UpdateColumnText()
        {
            if (_component == null) return;

            string? name = _component.Resource?.Name;

            if (name == null)
            {
                ColumnText1 = "Null";
                ColumnText2 = "";
            }
            else
            {
                ColumnText1 = name;

                const int MAX_DECIMAL_PLACES = 16;
                int decimalPlaces = Math.Min(_settingsProvider.DecimalPlacesInGraphNodes, MAX_DECIMAL_PLACES);
                var costStringMap = InfoVisualisationHelper.GetMappingForFloatsString(decimalPlaces);
                ColumnText2 = $" x{AbsCost.ToString(costStringMap)}";
            }
        }

        public void Dispose()
        {
            _resourceNameUpdateSubscription.Dispose();
            _valueUpdateSubscription.Dispose();
        }
    }
}
