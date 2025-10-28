using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
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

namespace Partlyx.UI.Avalonia.DataTemplateSelectors
{
    public class WindowDialogsTemplateSelector : IDataTemplate
    {
        public DataTemplate? PartsSelectionWindowTemplate { get; set; }

        public Control Build(object? item)
        {
            if (PartsSelectionWindowTemplate != null &&
                (item is PartsSelectionWindowViewModel<ResourceViewModel> ||
                 item is PartsSelectionWindowViewModel<RecipeViewModel> ||
                 item is PartsSelectionWindowViewModel<RecipeComponentViewModel> ||
                 item is PartsSelectionWindowViewModel<IVMPart>))
            {
                return PartsSelectionWindowTemplate?.Build(item);
            }

            return new TextBlock { Text = "No template found" };
        }

        public bool Match(object? item)
        {
            return item is PartsSelectionWindowViewModel<ResourceViewModel> ||
                   item is PartsSelectionWindowViewModel<RecipeViewModel> ||
                   item is PartsSelectionWindowViewModel<RecipeComponentViewModel> ||
                   item is PartsSelectionWindowViewModel<IVMPart>;
        }
    }
}
