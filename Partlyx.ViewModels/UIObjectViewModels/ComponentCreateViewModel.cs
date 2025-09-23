using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public class ComponentCreateViewModel
    {
        public IIsolatedSelectedParts SelectedParts { get; }
        public IResourceSearchService Search { get; }

        public ComponentCreateViewModel(IIsolatedSelectedParts isl, IResourceSearchService rss)
        {
            SelectedParts = isl;
            Search = rss;
        }
    }
}
