using Partlyx.Core.Settings;

namespace Partlyx.Core
{
    public abstract class GroupBase<TContent>
    {
        public string Name { get; set; } = "";
        public List<TContent> ContentList { get; set; } = new();
        public List<GroupBase<TContent>> SubGroups { get; set; } = new();

        public GroupBase<TContent> WithContent(IEnumerable<TContent> content)
        {
            ContentList.AddRange(content);
            return this;
        }
        public GroupBase<TContent> WithSubGroups(IEnumerable<GroupBase<TContent>> groups)
        {
            SubGroups.AddRange(groups);
            return this;
        }

        public List<TContent> GetAsOneLevelContentList()
        {
            var list = new List<TContent>();

            RecursiveAddToList(this);

            void RecursiveAddToList(GroupBase<TContent> group)
            {
                list.AddRange(group.ContentList);
                foreach (var subGroup in group.SubGroups)
                    RecursiveAddToList(subGroup);
            }

            return list;
        }

        public virtual ReadonlyGroup<TContent> ToReadOnlyGroup()
            => new ReadonlyGroup<TContent>(Name, ContentList, SubGroups);

        public TGroup ToSpecificGroup<TGroup>() where TGroup : GroupBase<TContent>
            => (TGroup)this;
    }
}
