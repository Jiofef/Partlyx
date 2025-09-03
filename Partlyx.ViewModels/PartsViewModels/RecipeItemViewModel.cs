using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services;
using Partlyx.Services.Dtos;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;

namespace Partlyx.ViewModels.PartsViewModels
{
    public class RecipeItemViewModel : ObservableObject
    {
        private readonly IRecipeComponentService _service;
        private readonly IVMPartsFactory _partsFactory;
        public Guid Uid { get; }
        public RecipeItemViewModel(RecipeDto dto, IRecipeComponentService service, IVMPartsFactory partsFactory)
        {
            Uid = dto.Uid;

            _service = service;
            _partsFactory = partsFactory;

            _craftAmount = dto.CraftAmount;

            foreach (var component in dto.Components)
            {
                var vm = _partsFactory.CreateRecipeComponentVM(component);
                _components.Add(vm);
            }
        }
        private double _craftAmount;
        public double CraftAmount { get => _craftAmount; set => SetProperty(ref _craftAmount, value); }

        private ObservableCollection<RecipeComponentItemViewModel> _components = new();
        public ObservableCollection<RecipeComponentItemViewModel> Components { get => _components; }

        public void UpdateFromDto(RecipeDto dto)
        {
            CraftAmount = dto.CraftAmount;
        }
    }
}
