using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels
{
    public abstract class UpdatableViewModel<TDto> : ObservableObject
    {
        private readonly Dictionary<string, Action<TDto>> _updaters;
        protected UpdatableViewModel()
        {
            _updaters = ConfigureUpdaters();
        }

        protected abstract Dictionary<string, Action<TDto>> ConfigureUpdaters();

        public void Update(TDto dto, IReadOnlyList<string>? changedProperties = null)
        {
            if (changedProperties == null)
            {
                foreach (var updater in _updaters.Values)
                    updater(dto);
            }
            else
            {
                foreach (var prop in changedProperties)
                    if (_updaters.TryGetValue(prop, out var updater))
                        updater(dto);
            }
        }
    }
}
