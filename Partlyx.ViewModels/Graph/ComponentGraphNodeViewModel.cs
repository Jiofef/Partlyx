﻿using DynamicData.Binding;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UJL.CSharp.Collections;

namespace Partlyx.ViewModels.Graph
{
    public class ComponentGraphNodeViewModel : GraphTreeNodeViewModel, IDisposable
    {
        private readonly IDisposable _resourceNameUpdateSubscription;
        private readonly IDisposable _quantityUpdateSubscription;
        private readonly IDisposable _craftAmountUpdateSubscription;
        private readonly IDisposable _valueUpdateSubscription;

        private string _columnText = "";
        public string ColumnText { get => _columnText; private set => SetProperty(ref _columnText, value); }

        public ComponentGraphNodeViewModel(RecipeComponentViewModel value)
            : base(value.Uid,
              (value.SelectedRecipeComponents != null
              ? new ObservableCollectionProjection<Guid, RecipeComponentViewModel>(value.SelectedRecipeComponents, (component => component.Uid))
              : null),
              value)
        {
            if (Value is RecipeComponentViewModel component)
            {
                _valueComponent = component;
            }

            _resourceNameUpdateSubscription =
                this.WhenAnyValue((@this => @this._valueComponent.LinkedResource.Value.Name))
                .Subscribe((o) => UpdateColumnText());

            _valueUpdateSubscription = 
                this.WhenValueChanged(@this => @this.Value)
                .Subscribe((o) => OnValueChanged());

            _quantityUpdateSubscription =
                this.WhenAnyValue(@this => @this._valueComponent.Quantity)
                .Subscribe((o) => OnCostChanged());

            _craftAmountUpdateSubscription =
                this.WhenAnyValue(@this => @this._valueComponent.LinkedParentRecipe.Value.CraftAmount)
                .Subscribe((o) => OnCostChanged());
        }

        private RecipeComponentViewModel? _valueComponent = null;
        private void OnValueChanged()
        {
            if (Value is RecipeComponentViewModel component)
            {
                _valueComponent = component;
            }
            OnCostChanged();
        }
        private double _localCost;

        private double _cost;
        public double Cost { get => _cost; private set => SetProperty(ref _cost, value); }

        protected override void Build()
        {
            base.Build();

            UpdateCost();
        }

        private void UpdateCost()
        {
            if (_valueComponent != null)
            {
                if (Parent is RecipeGraphNodeViewModel || _valueComponent.LinkedParentRecipe?.Value?.CraftAmount == null)
                    _localCost = _valueComponent.Quantity;
                else
                    _localCost = _valueComponent.Quantity / _valueComponent.LinkedParentRecipe.Value.CraftAmount;
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
            if (_valueComponent == null) return;

            string? name = _valueComponent.LinkedResource?.Value?.Name;

            if (name == null)
                ColumnText = "Null";
            else
            {
                string newText = name;

                newText += $" x{Cost}";
                ColumnText = newText;
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
