using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services;
using Partlyx.Services.Dtos;
using System.ComponentModel.Design;

namespace Partlyx.ViewModels.PartsViewModels
{
    public class RecipeComponentItemViewModel : ObservableObject
    {
        private readonly IRecipeService _service;
        private readonly IVMPartsFactory _partsFactory;
        public Guid Uid { get; }
        public RecipeComponentItemViewModel(RecipeComponentDto dto, IRecipeService service, IVMPartsFactory partsFactory)
        {
            Uid = dto.Uid;

            _service = service;
            _partsFactory = partsFactory;

            _quantity = dto.Quantity;
        }

        private Guid _resourceUid;
        public Guid ResourceUid { get => _resourceUid; set => SetProperty(ref _resourceUid, value); }

        private double _quantity;
        public double Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

        public void UpdateFromDto(RecipeComponentDto dto)
        {
            Quantity = dto.Quantity;
        }
    }
}
