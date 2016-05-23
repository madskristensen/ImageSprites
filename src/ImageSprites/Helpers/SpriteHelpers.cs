using System;
using System.Drawing.Imaging;
using System.IO;

namespace ImageSprites
{
    /// <summary>
    /// A collection of methods that are helpful for working with image sprites.
    /// </summary>
    public static class SpriteHelpers
    {
        /// <summary>
        /// Calculates the relative path between two files.
        /// </summary>
        public static string MakeRelative(string fileBase, string file)
        {
            Uri one = new Uri(fileBase);
            Uri two = new Uri(file);

            return one.MakeRelativeUri(two).ToString().Replace("\\", "/");
        }

        /// <summary>
        /// Calculates an identifier based on the provided file.
        /// </summary>
        public static string GetIdentifier(string imageFile)
        {
            return Path.GetFileNameWithoutExtension(imageFile)
                       .ToLowerInvariant()
                       .Replace(" ", string.Empty);
        }

        internal static ImageFormat ExtensionFromFormat(ImageType format)
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

        internal static ImageType GetImageFormatFromExtension(string file)
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