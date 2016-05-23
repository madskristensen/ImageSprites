using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSprites
{
    /// <summary>
    /// A generator for producing image sprites.
    /// </summary>
    public class SpriteGenerator
    {
        /// <summary>
        /// Generates an image sprite based on the specified <see cref="SpriteDocument"/>.
        /// </summary>
        public async Task Generate(SpriteDocument doc)
        {
            var images = GetImages(doc);

            int width = doc.Orientation == Orientation.Vertical ? images.Values.Max(i => i.Width) + (doc.Padding * 2) : images.Values.Sum(i => i.Width) + (doc.Padding * images.Count) + doc.Padding;
            int height = doc.Orientation == Orientation.Vertical ? images.Values.Sum(img => img.Height) + (doc.Padding * images.Count) + doc.Padding : images.Values.Max(img => img.Height) + (doc.Padding * 2);

            List<SpriteFragment> fragments = new List<SpriteFragment>();

            using (var bitmap = new Bitmap(width, height))
            {
                bitmap.SetResolution(doc.Dpi, doc.Dpi);

                using (Graphics canvas = Graphics.FromImage(bitmap))
                {
                    if (doc.Orientation == Orientation.Vertical)
                        Vertical(images, fragments, canvas, doc.Padding);
                    else
                        Horizontal(images, fragments, canvas, doc.Padding);
                }

                string outputFile = doc.FileName + doc.OutputExtension;

                OnSaving(outputFile, doc);
                bitmap.Save(outputFile, SpriteHelpers.ExtensionFromFormat(doc.Output));
                OnSaved(outputFile, doc);
            }

            await SpriteExporter.ExportStylesheet(fragments, doc, this);
        }

        private static void Vertical(Dictionary<string, Bitmap> images, List<SpriteFragment> fragments, Graphics canvas, int margin)
        {
            int currentY = margin;

            foreach (string ident in images.Keys)
            {
                var img = images[ident];
                fragments.Add(new SpriteFragment(ident, img.Width, img.Height, margin, currentY));

                canvas.DrawImage(img, margin, currentY);
                currentY += img.Height + margin;
            }
        }

        private static void Horizontal(Dictionary<string, Bitmap> images, List<SpriteFragment> fragments, Graphics canvas, int margin)
        {
            int currentX = margin;

            foreach (string ident in images.Keys)
            {
                var img = images[ident];
                fragments.Add(new SpriteFragment(ident, img.Width, img.Height, currentX, margin));

                canvas.DrawImage(img, currentX, margin);
                currentX += img.Width + margin;
            }
        }

        private static Dictionary<string, Bitmap> GetImages(SpriteDocument doc)
        {
            var images = new Dictionary<string, Bitmap>();
            var source = doc.ToAbsoluteImages();

            foreach (string ident in source.Keys)
            {
                string file = source[ident];

                if (!File.Exists(file))
                {
                    throw new FileNotFoundException("One or more sprite input files don't exist", file);
                }

                var bitmap = (Bitmap)Image.FromFile(file);
                bitmap.SetResolution(doc.Dpi, doc.Dpi);

                images.Add(ident, bitmap);
            }

            return images;
        }

        internal void OnSaving(string fileName, SpriteDocument doc)
        {
            Saving?.Invoke(this, new SpriteImageGenerationEventArgs(fileName, doc));
        }

        internal void OnSaved(string fileName, SpriteDocument doc)
        {
            Saved?.Invoke(this, new SpriteImageGenerationEventArgs(fileName, doc));
        }

        /// <summary>Fires before a file is written to disk.</summary>
        public event SpriteImageGenerationEventHandler Saving;

        /// <summary>Fires after a file is written to disk.</summary>
        public event SpriteImageGenerationEventHandler Saved;
    }
}
