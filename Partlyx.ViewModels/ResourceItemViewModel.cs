using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Dtos;
using System.ComponentModel.Design;

namespace Partlyx.ViewModels
{
    public class ResourceItemViewModel : ObservableObject
    {
        private readonly IResourceService _service;
        public Guid Uid { get; }
        public ResourceItemViewModel(ResourceDto dto, IResourceService service)
        {
            Uid = dto.Uid;
            Name = dto.Name;

            _service = service;
        }
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public void UpdateFromDto(ResourceDto dto)
        {
            Name = dto.Name;
        }
    }
}
