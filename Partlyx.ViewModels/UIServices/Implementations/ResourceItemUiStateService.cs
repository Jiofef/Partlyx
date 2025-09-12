using Microsoft.Extensions.DependencyInjection;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public class ResourceItemUiStateService : IResourceItemUiStateService
    {
        private IServiceProvider _provider { get; }
        public ResourceItemUiStateService(IServiceProvider provider) 
        {
            _provider = provider;
        }
        private readonly Dictionary<Guid, ResourceItemUIState> _states = new();

        public ResourceItemUIState GetOrCreate(ResourceItemViewModel vm)
        {
            var state = _states.GetValueOrDefault(vm.Uid);
            if (state == null)
            {
                state = (ResourceItemUIState)ActivatorUtilities.CreateInstance(_provider, typeof(ResourceItemUIState), vm);
                _states.Add(vm.Uid, state);
            }
            return state;
        }
    }
}
