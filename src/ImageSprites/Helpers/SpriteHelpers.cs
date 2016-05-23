using System;
using System.Drawing.Imaging;
using System.IO;

namespace ImageSprites
{
    public static class SpriteHelpers
    {
        public static string MakeRelative(string fileBase, string file)
        {
            Uri one = new Uri(fileBase);
            Uri two = new Uri(file);

            return one.MakeRelativeUri(two).ToString().Replace("\\", "/");
        }

        public static string GetIdentifier(string imageFile)
        {
            return Path.GetFileNameWithoutExtension(imageFile)
                       .ToLowerInvariant()
                       .Replace(" ", string.Empty);
        }

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