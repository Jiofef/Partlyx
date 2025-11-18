using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Partlyx.Core.Help;
using Partlyx.Core.Settings;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Partlyx.ViewModels.Info
{
    public class InfoSectionsGroupViewModel : ObservableObject
    {
        public InfoSectionsGroupViewModel(ReadonlyGroup<InfoSection> scheme)
        {
            // This constructor recursively transforms the core structure, taking into account the child groups and their options
            Name = scheme.Name;
            Sections = new(scheme.ContentList.Select(o => InfoSectionViewModel.Create(o)));
            SubGroups = new(scheme.SubGroups.Select(g => new InfoSectionsGroupViewModel(g)));

            SectionsAndGroups.AddRange(Sections);
            SectionsAndGroups.AddRange(SubGroups);
        }

        public List<InfoSectionViewModel> ToOneLevelOptionsList()
        {
            var list = new List<InfoSectionViewModel>();

            RecursiveAddToList(this);

            void RecursiveAddToList(InfoSectionsGroupViewModel group)
            {
                list.AddRange(group.Sections);
                foreach (var subGroup in group.SubGroups)
                    RecursiveAddToList(subGroup);
            }

            return list;
        }

        private InfoSectionsGroupViewModel() { }
        private string _name = "";
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public ObservableCollection<InfoSectionsGroupViewModel> SubGroups { get; set; } = new();
        public ObservableCollection<InfoSectionViewModel> Sections { get; set; } = new();

        public ObservableCollection<object> SectionsAndGroups { get; set; } = new();

        public InfoSectionViewModel? GetDescendantSectionOrNull(string sectionKey)
        {
            InfoSectionViewModel? result = null;

            RecursiveSearch(this);

            void RecursiveSearch(InfoSectionsGroupViewModel parentGroup)
            {
                foreach(var section in parentGroup.Sections)
                    if (section.Key == sectionKey)
                    {
                        result = section;
                        return;
                    }

                foreach(var group in parentGroup.SubGroups)
                    RecursiveSearch(group);
            }

            return result;
        }

        public InfoSectionsGroupViewModel Duplicate() => new InfoSectionsGroupViewModel()
        {
            Name = Name,
            SubGroups = new ObservableCollection<InfoSectionsGroupViewModel>(SubGroups.Select(g => g.Duplicate())),
            Sections = new ObservableCollection<InfoSectionViewModel>(Sections.Select(g => g.Duplicate()))
        };
    }
}