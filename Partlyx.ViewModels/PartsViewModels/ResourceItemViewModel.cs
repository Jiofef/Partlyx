using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Dtos;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;

namespace Partlyx.ViewModels.PartsViewModels
{
    public class ResourceItemViewModel : ObservableObject
    {
        private readonly IResourceService _service;
        private readonly IVMPartsFactory _partsFactory;
        public Guid Uid { get; }
        public ResourceItemViewModel(ResourceDto dto, IResourceService service, IVMPartsFactory partsFactory)
        {
            Uid = dto.Uid;

            _service = service;
            _partsFactory = partsFactory;

            _name = dto.Name;

            foreach (var recipe in dto.Recipes)
            {
                var vm = _partsFactory.CreateRecipeVM(recipe);
                _recipes.Add(vm);
            }
        }
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private ObservableCollection<RecipeItemViewModel> _recipes = new();
        public ObservableCollection<RecipeItemViewModel> Recipes { get => _recipes; }

        //pub

        public void UpdateFromDto(ResourceDto dto)
        {
            Name = dto.Name;
        }
    }
}
