using System.Drawing.Imaging;

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
    }
}