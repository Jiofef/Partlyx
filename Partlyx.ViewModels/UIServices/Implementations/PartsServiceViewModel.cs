namespace Partlyx.ViewModels.UIServices.Implementations
{
    public class PartsServiceViewModel
    {
        public PartsServiceViewModel(ResourceServiceViewModel resService, RecipeServiceViewModel recService, RecipeComponentServiceViewModel comService) 
        {
            ResourceService = resService;
            RecipeService = recService;
            ComponentService = comService;
        }

        public ResourceServiceViewModel ResourceService { get; }
        public RecipeServiceViewModel RecipeService { get; }
        public RecipeComponentServiceViewModel ComponentService { get; }
    }
}
