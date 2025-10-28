using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.VMImplementations
{
    public class ImageAwesomeIconCatalog : IIconFigureCatalog
    {
        private readonly List<IconFigureItem> _cache;
        public ImageAwesomeIconCatalog()
        {
            _cache = new List<IconFigureItem>();
            foreach (var e in Enum.GetValues(typeof(Meziantou.WpfFontAwesome.FontAwesomeSolidIcon)).Cast<object>())
                _cache.Add(new IconFigureItem("FontAwesome", "Solid", e.ToString() ?? ""));
        }
        public IReadOnlyList<IconItem> GetAll() => _cache;
    }

}
