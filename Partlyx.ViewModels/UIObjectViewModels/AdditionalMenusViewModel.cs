namespace Partlyx.ViewModels.UIObjectViewModels
{
    public class AdditionalMenusViewModel
    {
        public ItemPropertiesViewModel PartProperties { get; }
        public ResourceConverterViewModel ResourceConverter { get; }
        public AdditionalMenusViewModel(ItemPropertiesViewModel partProperties, ResourceConverterViewModel converter) 
        {
            PartProperties = partProperties;
            ResourceConverter = converter;
        }
    }
}
