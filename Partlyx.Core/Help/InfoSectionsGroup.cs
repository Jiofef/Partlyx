namespace Partlyx.Core.Help
{
    public class InfoSectionsGroup : GroupBase<InfoSection>
    {
        public List<InfoSection> Sections { get => ContentList; set => ContentList = value; }

        public InfoSectionsGroup(string name)
        {
            Name = name;
        }
        public InfoSectionsGroup WithSections(IEnumerable<InfoSection> sections)
            => (InfoSectionsGroup)WithContent(sections);
        public List<InfoSection> GetAsOneLevelOptionsList() => GetAsOneLevelContentList();

        new public InfoSectionsGroup WithSubGroups(IEnumerable<GroupBase<InfoSection>> groups)
            => (InfoSectionsGroup)base.WithSubGroups(groups);
    }
}
