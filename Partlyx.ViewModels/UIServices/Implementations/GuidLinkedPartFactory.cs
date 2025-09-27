using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public class GuidLinkedPartFactory : IGuidLinkedPartFactory
    {
        private readonly IServiceProvider _services;
        public GuidLinkedPartFactory(IServiceProvider services)
        {
            _services = services;
        }

        public GuidLinkedPart<TPart> CreateLinkedPart<TPart>(Guid uid) where TPart : IVMPart
        {
            var result = (GuidLinkedPart<TPart>)ActivatorUtilities.CreateInstance(_services, typeof(GuidLinkedPart<TPart>), uid!);
            return result;
        }
    }
}
