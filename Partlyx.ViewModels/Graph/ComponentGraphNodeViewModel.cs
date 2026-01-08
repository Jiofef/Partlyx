using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using ReactiveUI;
using UJL.CSharp.Collections;

namespace Partlyx.ViewModels.Graph
{
    public class ComponentGraphNodeViewModel : GraphNodeViewModel, IDisposable, ITypedVMPartHolder<RecipeComponentViewModel>
    {
        private readonly IDisposable _resourceNameUpdateSubscription;
        private readonly IDisposable _quantityUpdateSubscription;
        private readonly IDisposable _valueUpdateSubscription;

        private string _columnTextPart1 = "";
        private string _columnTextPart2 = "";
        public string ColumnTextPart1 { get => _columnTextPart1; private set => SetProperty(ref _columnTextPart1, value); }
        public string ColumnTextPart2 { get => _columnTextPart2; private set => SetProperty(ref _columnTextPart2, value); }

        public ComponentGraphNodeViewModel(RecipeComponentViewModel value, GraphNodeViewModel? mainRelative = null)
            : base(mainRelative, value)
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

        private void UpdateCost()
        {
            if (_component == null)
            {
                Cost = 1;
                UpdateColumnText();
                return;
            }

            double totalProduced = 0;

            foreach (var parent in Parents)
            {
                if (parent is RecipeGraphNodeViewModel recipeNode && recipeNode.Value is RecipeViewModel recipe)
                {
                    // Find this component in recipe outputs
                    var outputComponent = recipe.Outputs.FirstOrDefault(c => c.Resource == _component.Resource);
                    if (outputComponent != null)
                    {
                        // Calculate recipe scale based on input components that are also parents
                        double recipeScale = 0;
                        foreach (var input in recipe.Inputs)
                        {
                            var inputParent = Parents.FirstOrDefault(p =>
                                p is ComponentGraphNodeViewModel compNode &&
                                compNode._component?.Resource == input.Resource);
                            if (inputParent is ComponentGraphNodeViewModel compNode && compNode._component != null)
                            {
                                recipeScale += compNode.Cost / compNode._component.Quantity * input.Quantity;
                            }
                        }

                        if (recipeScale > 0)
                        {
                            totalProduced += recipeScale / recipe.Inputs.Sum(i => i.Quantity) * outputComponent.Quantity;
                        }
                    }
                }
                else if (parent is ComponentGraphNodeViewModel parentComponent && parentComponent._component != null)
                {
                    // Direct component transformation
                    totalProduced += parentComponent.Cost / parentComponent._component.Quantity * _component.Quantity;
                }
            }

            Cost = totalProduced > 0 ? totalProduced : _component.Quantity;
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
            _valueUpdateSubscription.Dispose();
        }
    }
}
