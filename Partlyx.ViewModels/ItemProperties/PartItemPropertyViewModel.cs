using CommunityToolkit.Mvvm.Input;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.DragAndDrop.Implementations;
using ReactiveUI;

namespace Partlyx.ViewModels.ItemProperties
{
    public partial class PartItemPropertyViewModel<TPart> : ItemPropertyViewModel, IDisposable where TPart : IVMPart
    {
        private TPart? _part;
        public TPart? Part { get => _part; set => SetProperty(ref _part, value); }

        private bool _allowUnselected;
        public bool AllowUnselected { get => _allowUnselected; set => SetProperty(ref _allowUnselected, value); }

        private SinglePartContainerDropHandlerViewModel<TPart>? _dropHandler;
        /// <summary>
        /// Part in the DropHandler is only updated when the Drop is performed, so for up-to-date values, reference the Part from PartItemPropertyViewModel
        /// </summary>
        public SinglePartContainerDropHandlerViewModel<TPart>? DropHandler { get => _dropHandler; set => SetProperty(ref _dropHandler, value); }

        public Func<object?, Task>? SelectButtonPressedTask { get; set; }

        private IDisposable? _dropHandlerPartChangedSubscription;
        public PartItemPropertyViewModel()
        {
            var dropHandlerChangedSubscription = this.WhenAnyValue(t => t.DropHandler).Subscribe(_ =>
            {
                _dropHandlerPartChangedSubscription?.Dispose();
                _dropHandlerPartChangedSubscription = DropHandler?.WhenAnyValue(d => d.Part).Subscribe(_ =>
                {
                    Task.Run(async () =>
                    {
                        await SaveChanges(DropHandler.Part);
                    });
                });
            });
            Subscriptions.Add(dropHandlerChangedSubscription);
        }

        public override void Dispose()
        {
            base.Dispose();
            _dropHandlerPartChangedSubscription?.Dispose();
        }

        [RelayCommand]
        public async Task SelectButtonPressed(object? args)
        {
            if (SelectButtonPressedTask != null)
                await SelectButtonPressedTask(args);
        }
    }

    public class ResourceItemPropertyViewModel : PartItemPropertyViewModel<ResourceViewModel> 
    {
        public ResourceItemPropertyViewModel()
        {
            DropHandler = new SingleResourceContainerDropHandlerViewModel();
        }
    }
    public class RecipeItemPropertyViewModel : PartItemPropertyViewModel<RecipeViewModel> 
    {
        public RecipeItemPropertyViewModel()
        {
            DropHandler = new SingleRecipeContainerDropHandlerViewModel();
        }
    }
    public class RecipeComponentItemPropertyViewModel : PartItemPropertyViewModel<RecipeComponentViewModel>
    {
        public RecipeComponentItemPropertyViewModel()
        {
            DropHandler = new SingleRecipeComponentContainerDropHandlerViewModel();
        }
    }
}
