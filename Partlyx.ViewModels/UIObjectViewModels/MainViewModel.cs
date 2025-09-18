using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public class MainViewModel
    {
        public IGlobalSelectedParts SelectedParts { get; }

        public ResourceListViewModel ResourceList { get; }
        public RecipeListViewModel RecipeList { get; }
        public RecipeComponentListViewModel RecipeComponents { get; }

        public PartsTreeViewModel PartsTree { get; }

        public MenuPanelViewModel MenuPanel { get; }

        private readonly IPartsLoader _partsLoader;
        private readonly IPartsInitializeService _partsInitializeService;

        public MainViewModel(
            ResourceListViewModel resourceList, RecipeListViewModel recipeList, 
            RecipeComponentListViewModel recipeComponents, PartsTreeViewModel partsTree,
            MenuPanelViewModel menuPanel,
            IGlobalSelectedParts selectedParts,
            IPartsLoader pl,
            IPartsInitializeService pis
            )
        {
            ResourceList = resourceList;
            RecipeList = recipeList;
            RecipeComponents = recipeComponents;
            PartsTree = partsTree;
            MenuPanel = menuPanel;
            SelectedParts = selectedParts;

            _partsLoader = pl;
            _partsInitializeService = pis;
        }
    }
}
