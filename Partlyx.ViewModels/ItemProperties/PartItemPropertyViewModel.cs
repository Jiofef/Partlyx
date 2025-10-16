using CommunityToolkit.Mvvm.Input;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.ItemProperties
{
    public partial class PartItemPropertyViewModel<TPart> : ItemPropertyViewModel where TPart : IVMPart
    {
        private TPart? _part;

        public TPart? Part { get => _part; set => SetProperty(ref _part, value); }

        private bool _allowUnselected;
        public bool AllowUnselected { get => _allowUnselected; set => SetProperty(ref _allowUnselected, value); }

        public Func<object?, Task>? SelectButtonPressedTask { get; set; }


        [RelayCommand]
        public async Task SelectButtonPressed(object? args)
        {
            if (SelectButtonPressedTask != null)
                await SelectButtonPressedTask(args);
        }

        public PartItemPropertyViewModel() 
        {

        }
    }

    public class ResourceItemPropertyViewModel : PartItemPropertyViewModel<ResourceViewModel> { }
    public class RecipeItemPropertyViewModel : PartItemPropertyViewModel<RecipeViewModel> { }
    public class RecipeComponentItemPropertyViewModel : PartItemPropertyViewModel<RecipeComponentViewModel> { }
}
