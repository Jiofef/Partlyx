using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface IVMPart : IDisposable
    {
        PartTypeEnumVM PartType { get; }
        Guid Uid { get; }
    }

    public enum PartTypeEnumVM { Resource, Recipe, Component }
}
