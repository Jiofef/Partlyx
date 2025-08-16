
namespace Partlyx.Core
{
    public interface ICopiable<To>
    {
        public ICopiable<To> CopyTo(To insertPlace);
    }
}
