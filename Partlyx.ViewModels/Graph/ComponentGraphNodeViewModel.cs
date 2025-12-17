using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using ReactiveUI;
using UJL.CSharp.Collections;

namespace Partlyx.ViewModels.Graph
{
    public class ComponentGraphNodeViewModel : GraphTreeNodeViewModel, IDisposable, ITypedVMPartHolder<RecipeComponentViewModel>
    {
        private readonly IDisposable _resourceNameUpdateSubscription;
        private readonly IDisposable _quantityUpdateSubscription;
        private readonly IDisposable _craftAmountUpdateSubscription;
        private readonly IDisposable _valueUpdateSubscription;

        private string _columnTextPart1 = "";
        private string _columnTextPart2 = "";
        public string ColumnTextPart1 { get => _columnTextPart1; private set => SetProperty(ref _columnTextPart1, value); }
        public string ColumnTextPart2 { get => _columnTextPart2; private set => SetProperty(ref _columnTextPart2, value); }

        public ComponentGraphNodeViewModel(RecipeComponentViewModel value)
            : base(value.Uid,
              (value.SelectedRecipeComponents != null
              ? new ObservableCollectionProjection<Guid, RecipeComponentViewModel>(value.SelectedRecipeComponents, (component => component.Uid))
              : null),
              value)
        {
            if (Value is RecipeComponentViewModel component)
            {
                _component = component;
            }

            _resourceNameUpdateSubscription =
                this.WhenAnyValue((@this => @this._component.LinkedResource.Value.Name))
                .Subscribe((o) => UpdateColumnText());

            _valueUpdateSubscription = 
                this.WhenAnyValue(@this => @this.Value)
                .Subscribe((o) => OnValueChanged());

            _quantityUpdateSubscription =
                this.WhenAnyValue(@this => @this._component.Quantity)
                .Subscribe((o) => OnCostChanged());

            _craftAmountUpdateSubscription =
                this.WhenAnyValue(@this => @this._component.LinkedParentRecipe.Value.CraftAmount)
                .Subscribe((o) => OnCostChanged());
        }

        private void OnValueChanged()
        {
            if (Value is RecipeComponentViewModel component)
            {
                Part = component;
            }
            OnCostChanged();
        }
        private double _localCost;

        private double _cost;
        public double Cost { get => _cost; private set => SetProperty(ref _cost, value); }

        public PartTypeEnumVM? PartType => PartTypeEnumVM.Component;
        private RecipeComponentViewModel? _component = null;
        public RecipeComponentViewModel? Part { get => _component; private set => SetProperty(ref _component, value); }

        protected override void Build()
        {
            base.Build();

            UpdateCost();
        }

        private void UpdateCost()
        {
            if (_component != null)
            {
                if (Parent is RecipeGraphNodeViewModel || _component.LinkedParentRecipe?.Value?.CraftAmount == null)
                    _localCost = _component.Quantity;
                else
                    _localCost = _component.Quantity / _component.LinkedParentRecipe.Value.CraftAmount;
            }
            else
                _localCost = 1;

            var parentComponent = TryFindParent<ComponentGraphNodeViewModel>();
            if (parentComponent != null)
            {
                Cost = _localCost * parentComponent.Cost;
            }
            else
                Cost = _localCost;

            UpdateColumnText();
        }
        private void OnCostChanged()
        {
            UpdateCost();
            
            ExcecuteWithAllTheChildren((child) =>
            {
                if (child is ComponentGraphNodeViewModel componentNode)
                {
                    componentNode.UpdateCost();
                }
            });
        }

        private void UpdateColumnText()
        {
            if (_component == null) return;

            string? name = _component.LinkedResource?.Value?.Name;

            if (name == null)
            {
                ColumnTextPart1 = "Null";
                ColumnTextPart2 = "";
            }
            else
            {
                ColumnTextPart1 = name;
                ColumnTextPart2 = $" x{Cost}";
            }
        }

        public void Dispose()
        {
            _resourceNameUpdateSubscription.Dispose();
            _quantityUpdateSubscription.Dispose();
            _craftAmountUpdateSubscription.Dispose();
            _valueUpdateSubscription.Dispose();
        }
    }
}
