using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Implementations;
using ReactiveUI;
namespace Partlyx.ViewModels.UIStates
{
    public partial class RecipeComponentNodeUIState : ObservableObject, IDisposable
    {
        private readonly PartsServiceViewModel _services;

        private readonly RecipeComponentItemViewModel _componentVM;

        private readonly IDisposable _columnTextUpdateSubscription;

        public RecipeComponentNodeUIState(RecipeComponentItemViewModel vm, PartsServiceViewModel svm)
        {
            _services = svm;

            _componentVM = vm;

            _columnTextUpdateSubscription = _componentVM
                .WhenAnyValue(c => c.LinkedResource!.Value!.Name)
                .Subscribe(n => UpdateColumnText());

            UpdateColumnText();
        }

        private string _columnText = "";
        public string ColumnText { get => _columnText; private set => SetProperty(ref _columnText, value); }

        private void UpdateColumnText()
        {
            string? name = _componentVM.LinkedResource?.Value?.Name;

            if (name == null)
                ColumnText = "Null";
            else
            {
                string newText = name;

                // When different component display modes appear, the code here should be updated
                newText += $" x{_componentVM.Quantity}";
                ColumnText = newText;
            }
        }

        public void Dispose()
        {
            _columnTextUpdateSubscription.Dispose();
        }
    }
}
