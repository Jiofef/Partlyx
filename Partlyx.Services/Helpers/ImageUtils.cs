using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;

namespace Partlyx.Services.Helpers
{
    public static class ImageUtils
    {
        /// <summary>
        /// Resize the image to fit within the specified target width and height,
        /// preserving the aspect ratio. No padding or transparent canvas is added.
        /// Returns a byte array and MIME type.
        /// Format: "png", "jpeg"|"jpg" or "webp".
        /// </summary>
        public static byte[] Resize(byte[] originalBytes, int targetWidth, int targetHeight, string mime = "image/png", int jpegQuality = 85)
        {
            if (originalBytes == null) throw new ArgumentNullException(nameof(originalBytes));
            if (targetWidth <= 0 || targetHeight <= 0) throw new ArgumentException("Target size must be > 0");

            using var image = Image.Load<Rgba32>(originalBytes); // auto-detect format
            int srcW = image.Width;
            int srcH = image.Height;

            // Calculate ratio to fit target while preserving aspect ratio
            double ratio = Math.Min((double)targetWidth / srcW, (double)targetHeight / srcH);
            ratio = Math.Min(1.0, ratio); // prevent upscaling

            int newW = Math.Max(1, (int)Math.Round(srcW * ratio));
            int newH = Math.Max(1, (int)Math.Round(srcH * ratio));

            // Resize using high-quality resampler
            image.Mutate(ctx => ctx.Resize(newW, newH, KnownResamplers.Lanczos3));

            using var ms = new MemoryStream();
            switch (mime)
            {
                case "image/jpeg":
                    var jpegEnc = new JpegEncoder { Quality = jpegQuality };
                    image.Save(ms, jpegEnc);
                    break;
                case "image/webp":
                    var webpEnc = new WebpEncoder();
                    image.Save(ms, webpEnc);
                    break;
                default:
                    image.Save(ms, new PngEncoder());
                    break;
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Converts a WebP byte array into PNG format.
        /// </summary>
        public static byte[] ConvertWebpToPng(byte[] webpBytes)
        {
            if (webpBytes == null) throw new ArgumentNullException(nameof(webpBytes));

            using var image = Image.Load<Rgba32>(webpBytes);
            using var ms = new MemoryStream();
            image.Save(ms, new PngEncoder());
            return ms.ToArray();
        }
    }

}
