using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Core.Settings;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Partlyx.ViewModels.Settings
{
    public class SettingsGroupViewModel : ObservableObject
    {
        public SettingsGroupViewModel(ReadonlyGroup<SchematicOption> scheme)
        {
            // This constructor recursively transforms the core structure, taking into account the child groups and their options
            Name = scheme.Name;
            Options = new(scheme.ContentList.Select(o => OptionViewModel.Create(o)));
            SubGroups = new(scheme.SubGroups.Select(g => new SettingsGroupViewModel(g)));
        }

        public List<OptionViewModel> ToOneLevelOptionsList()
        {
            var list = new List<OptionViewModel>();

            RecursiveAddToList(this);

            void RecursiveAddToList(SettingsGroupViewModel group)
            {
                list.AddRange(group.Options);
                foreach (var subGroup in group.SubGroups)
                    RecursiveAddToList(subGroup);
            }

            return list;
        }

        private SettingsGroupViewModel() { }
        private string _name = "";
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public ObservableCollection<SettingsGroupViewModel> SubGroups { get; set; } = new();
        public ObservableCollection<OptionViewModel> Options { get; set; } = new();

        public SettingsGroupViewModel Duplicate() => new SettingsGroupViewModel() 
        {
            Name = Name, 
            SubGroups = new ObservableCollection<SettingsGroupViewModel>(SubGroups.Select(g => g.Duplicate())),
            Options = new ObservableCollection<OptionViewModel>(Options.Select(g => g.Duplicate()))
        };
    }
}