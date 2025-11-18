using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Core.Help;

namespace Partlyx.ViewModels.Info
{
    public class InfoSectionViewModel : ObservableObject
    {
        private InfoSection _refInfo;
        private string _key;
        public string Key { get => _key; set => SetProperty(ref _key, value); }

        private string _contentKey;
        public string ContentKey { get => _contentKey; set => SetProperty(ref _contentKey, value); }

        public InfoSectionViewModel(InfoSection section)
        {
            _refInfo = section;
            _key = section.Key;
            _contentKey = section.ContentKey;
        }

        public static InfoSectionViewModel Create(InfoSection section)
            => new InfoSectionViewModel(section);
        public InfoSectionViewModel Duplicate()
            => new InfoSectionViewModel(_refInfo);
    }
}
