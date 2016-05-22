using System.Drawing.Imaging;
using System.IO;

namespace ImageSprites
{
    internal static class ImageHelpers
    {
        public static ImageFormat ExtensionFromFormat(ImageType format)
        {
            switch (format)
            {
                case ImageType.Jpg:
                    return ImageFormat.Jpeg;
                case ImageType.Gif:
                    return ImageFormat.Gif;
            }

            return ImageFormat.Png;
        }

        public static ImageType GetImageFormatFromExtension(string file)
        {
            string extension = Path.GetExtension(file);

            switch (extension.ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    return ImageType.Jpg;

                case ".gif":
                    return ImageType.Gif;
            }

            return ImageType.Png;
        }
    }
}