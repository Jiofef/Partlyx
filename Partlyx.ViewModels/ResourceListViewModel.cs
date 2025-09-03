using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Dtos;
using Partlyx.ViewModels.PartsViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels
{
    public class ResourceListViewModel : ObservableObject
    {
        public ObservableCollection<ResourceItemViewModel> Resources { get; } = new ObservableCollection<ResourceItemViewModel>();

        private void OnResourceChanged(object? sender, ResourceDto dto)
        {
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    var item = Resources.FirstOrDefault(r => r.Id == dto.Id);
            //    if (item != null)
            //        item.UpdateFromDto(dto);
            //    else
            //        Resources.Add(new ResourceItemViewModel(dto, _service));
            //});
        }
    }
}
