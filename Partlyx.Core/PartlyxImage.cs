namespace Partlyx.Core
{
    public class PartlyxImage
    {
        public PartlyxImage(Guid uid)
        {
            Uid = uid;
        }
        public PartlyxImage()
        {
            Uid = Guid.NewGuid();
        }
        public Guid Uid { get; }
        public string Name { get; set; } = "";
        public byte[] Content { get; set; } = new byte[0];
        public byte[] CompressedContent { get; set; } = new byte[0];
        public byte[] Hash { get; set; } = new byte[0];
        public string Mime { get; set; } = "image/png";
    }
}
