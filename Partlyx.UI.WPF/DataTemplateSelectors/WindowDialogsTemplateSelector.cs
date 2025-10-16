using Partlyx.ViewModels.ItemProperties;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIObjectViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Partlyx.UI.WPF.DataTemplateSelectors
{
    class WindowDialogsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? PartsSelectionWindowTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (PartsSelectionWindowTemplate != null)
            {
                if (item is PartsSelectionWindowViewModel<ResourceViewModel>) return PartsSelectionWindowTemplate;
                if (item is PartsSelectionWindowViewModel<RecipeViewModel>) return PartsSelectionWindowTemplate;
                if (item is PartsSelectionWindowViewModel<RecipeComponentViewModel>) return PartsSelectionWindowTemplate;
                if (item is PartsSelectionWindowViewModel<IVMPart>) return PartsSelectionWindowTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
