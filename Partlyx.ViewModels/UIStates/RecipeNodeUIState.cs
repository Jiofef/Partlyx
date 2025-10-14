using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData.Binding;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Implementations;
using ReactiveUI;

namespace Partlyx.ViewModels
{
    public partial class RecipeNodeUIState : ObservableObject, IDisposable
    {
        private readonly PartsServiceViewModel _services;
        private readonly RecipeItemViewModel _recipeVM;

        private readonly IDisposable _columnTextUpdateSubscription;
        private readonly IDisposable _secondaryColumnTextUpdateSubscription;


        public RecipeNodeUIState(RecipeItemViewModel vm, PartsServiceViewModel svm) 
        {
            _services = svm;

            _recipeVM = vm;

            _columnTextUpdateSubscription = _recipeVM
                .WhenValueChanged(rc => rc.Name)
                .Subscribe(n => UpdateColumnText());

            _secondaryColumnTextUpdateSubscription = _recipeVM
                .WhenAnyValue(rc => rc.LinkedParentResource!.Value!.Name)
                .Subscribe(n => UpdateSecondaryColumnText());

            UpdateColumnText();
            UpdateSecondaryColumnText();
        }

        private string _columnText = "";
        public string ColumnText { get => _columnText; private set => SetProperty(ref _columnText, value); }

        private string _secondaryColumnText = "";
        public string SecondaryColumnText { get => _secondaryColumnText; private set => SetProperty(ref _secondaryColumnText, value); }

        private void UpdateColumnText()
        {
            string? name = _recipeVM.Name;

            if (name == null)
                ColumnText = "Null";
            else
            {
                string newText = name;
                ColumnText = newText;
            }
        }
        private void UpdateSecondaryColumnText()
        {
            string? name = _recipeVM.LinkedParentResource?.Value?.Name;

            if (name == null)
                SecondaryColumnText = "Null";
            else
            {
                string newText = $"{name} x{_recipeVM.CraftAmount}";
                SecondaryColumnText = newText;
            }
        }

        public void Dispose()
        {
            _columnTextUpdateSubscription.Dispose();
            _secondaryColumnTextUpdateSubscription.Dispose();
        }
    }
}
