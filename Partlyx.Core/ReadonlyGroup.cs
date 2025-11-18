namespace Partlyx.Core.Settings
{
    public class ReadonlyGroup<TContent>
    {
        public ReadonlyGroup(string name, List<TContent> contentList, List<GroupBase<TContent>> subGroups)
        {
            Name = name;
            ContentList = contentList;
            SubGroups = subGroups.Select(sg => sg.ToReadOnlyGroup()).ToList(); // Making all the subgroups readonly
        }
        public string Name { get; }
        public IReadOnlyList<TContent> ContentList;
        public IReadOnlyList<ReadonlyGroup<TContent>> SubGroups;
    }
}
