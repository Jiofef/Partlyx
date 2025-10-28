using Material.Icons;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Partlyx.UI.Avalonia.VMImplementations
{
    public class MaterialIconCatalog : IIconFigureCatalog
    {
        private readonly List<IconFigureItem> _cache;
        public MaterialIconCatalog()
        {
            _cache = Enum.GetValues<MaterialIconKind>().Select(e => new IconFigureItem("Material", "Solid", e.ToString())).ToList();
        }
        public IReadOnlyList<IconItem> GetAll() => _cache;
    }
}