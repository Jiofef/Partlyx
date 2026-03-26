using Partlyx.ViewModels.Settings;

namespace Partlyx.ViewModels.UIServices
{
    public class GlobalInformationProvider
    {
        public ApplicationSettingsProviderViewModel ApplicationSettings { get; }
        public GlobalInformationProvider(ApplicationSettingsProviderViewModel settings) 
        {
            ApplicationSettings = settings;
        }
    }
}
