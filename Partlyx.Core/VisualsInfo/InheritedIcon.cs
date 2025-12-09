namespace Partlyx.Core.VisualsInfo
{
    /// <summary>
    /// An icon that copies an existing icon of another element
    /// </summary>
    public class InheritedIcon : IUidIcon
    {
        public InheritedIcon() { }

        public InheritedIcon(Guid uid, InheritedIconParentTypeEnum parentType) 
        {
            Uid = uid;
            ParentType = parentType;
        }
        /// <summary> Parent Uid </summary>
        public Guid Uid { get; set; }

        public InheritedIconParentTypeEnum ParentType { get; set; }
        public enum InheritedIconParentTypeEnum { Resource, Recipe }
    }
}
