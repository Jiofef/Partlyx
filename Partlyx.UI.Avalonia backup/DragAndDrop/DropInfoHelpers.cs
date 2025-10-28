using GongSolutions.Avalonia.DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.DragAndDrop
{
    public static class DropInfoHelpers
    {
        public static bool TryGetItemsOfType<T>(IDropInfo dropInfo, out List<T> items)
        {
            items = new List<T>();
            if (dropInfo == null) return false;

            if (dropInfo.Data is IEnumerable<T> data)
            {
                foreach (var item in data)
                    items.Add(item);
                return items.Count > 0;
            }

            else if (dropInfo.Data is T item)
            {
                items.Add(item);
                return true;
            }

            var srcItem = dropInfo.DragInfo?.SourceItem;
            if (srcItem is T item2)
            {
                items.Add(item2);
                return true;
            }

            return false;
        }
    }
}
