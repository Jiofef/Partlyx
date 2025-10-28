using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public interface IIconFigureCatalog
    {
        IReadOnlyList<IconItem> GetAll();
    }

    public record IconItem();
    public record IconFigureItem(string Library, string Style, string Name) : IconItem;
}
