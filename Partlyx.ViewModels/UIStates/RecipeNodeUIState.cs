using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData.Binding;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIObjectViewModels;
using Partlyx.ViewModels.UIServices.Implementations;
using ReactiveUI;

namespace Partlyx.ViewModels
{
    public partial class RecipeNodeUIState : ObservableObject, IDisposable
    {
        private readonly PartsServiceViewModel _services;
        private readonly RecipeViewModel _recipeVM;

        private readonly List<IDisposable> _subscriptions = new();


        public RecipeNodeUIState(RecipeViewModel vm, PartsServiceViewModel svm) 
        {
            _services = svm;

            _recipeVM = vm;

            var columnTextUpdateSubscription = _recipeVM
                .WhenAnyValue(rc => rc.Name)
                .Subscribe(n => UpdateColumnText());
            _subscriptions.Add(columnTextUpdateSubscription);

            var secondaryColumnTextUpdateSubscription = _recipeVM
                .WhenAnyValue(rc => rc.LinkedParentResource!.Value!.Name)
                .Subscribe(n => UpdateSecondaryColumnText());
            _subscriptions.Add(secondaryColumnTextUpdateSubscription);

            UpdateColumnText();
            UpdateSecondaryColumnText();
        }

        private string _columnText = "";
        public string ColumnText { get => _columnText; private set => SetProperty(ref _columnText, value); }

        private string _secondaryColumnTextPart1 = "";
        public string SecondaryColumnTextPart1 { get => _secondaryColumnTextPart1; private set => SetProperty(ref _secondaryColumnTextPart1, value); }
        private string _secondaryColumnTextPart2 = "";
        public string SecondaryColumnTextPart2 { get => _secondaryColumnTextPart2; private set => SetProperty(ref _secondaryColumnTextPart2, value); }

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
            {
                SecondaryColumnTextPart1 = "Null";
                SecondaryColumnTextPart2 = "";
            }
            else
            {
                SecondaryColumnTextPart1 = name;
                SecondaryColumnTextPart2 = $" x{_recipeVM.CraftAmount}";
            }
        }

        public void Dispose()
        {
            foreach(var subscription in _subscriptions)
                subscription.Dispose();
        }

        [RelayCommand]
        public void FindInTree()
        {
            _recipeVM.UiItem.FindInTree();
        }

        [RelayCommand]
        public void FindResourceInTree()
        {
            var resource = _recipeVM.LinkedParentResource?.Value;
            if (resource == null) return;

            resource.UiItem.FindInTree();
        }
    }
}
