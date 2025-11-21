namespace Partlyx.Services.Dtos
{
    public record ImageDto(Guid Uid, string Name, string Mime, byte[] Hash, byte[]? Content, byte[]? CompressedContent);
}
