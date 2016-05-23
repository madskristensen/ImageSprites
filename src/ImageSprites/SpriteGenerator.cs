using System;
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

            int width = doc.Orientation == Orientation.Vertical ? images.Values.Max(i => i.Item2.Width) + (doc.Padding * 2) : images.Values.Sum(i => i.Item2.Width) + (doc.Padding * images.Count) + doc.Padding;
            int height = doc.Orientation == Orientation.Vertical ? images.Values.Sum(img => img.Item2.Height) + (doc.Padding * images.Count) + doc.Padding : images.Values.Max(img => img.Item2.Height) + (doc.Padding * 2);

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

        private static void Vertical(Dictionary<string, Tuple<string, Bitmap>> images, List<SpriteFragment> fragments, Graphics canvas, int margin)
        {
            int currentY = margin;

            foreach (string ident in images.Keys)
            {
                var file = images[ident].Item1;
                var img = images[ident].Item2;
                fragments.Add(new SpriteFragment(ident, file, img.Width, img.Height, margin, currentY));

                canvas.DrawImage(img, margin, currentY);
                currentY += img.Height + margin;
            }
        }

        private static void Horizontal(Dictionary<string, Tuple<string, Bitmap>> images, List<SpriteFragment> fragments, Graphics canvas, int margin)
        {
            int currentX = margin;

            foreach (string ident in images.Keys)
            {
                var file = images[ident].Item1;
                var img = images[ident].Item2;
                fragments.Add(new SpriteFragment(ident, file, img.Width, img.Height, currentX, margin));

                canvas.DrawImage(img, currentX, margin);
                currentX += img.Width + margin;
            }
        }

        private static Dictionary<string, Tuple<string, Bitmap>> GetImages(SpriteDocument doc)
        {
            var images = new Dictionary<string, Tuple<string, Bitmap>>();
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

                images.Add(ident, Tuple.Create(file, bitmap));
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
