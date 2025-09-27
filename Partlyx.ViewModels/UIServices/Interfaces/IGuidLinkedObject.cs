namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IGuidLinkedObject<TValue>
    {
        public Guid Uid { get; }
        public TValue? Value { get; }
    }
}
