using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSprites
{
    public class SpriteGenerator
    {
        public async Task Generate(SpriteDocument doc)
        {
            Dictionary<string, Bitmap> images = GetImages(doc);

            int width = doc.Direction == Direction.Vertical ? images.Values.Max(i => i.Width) + (doc.Padding * 2) : images.Values.Sum(i => i.Width) + (doc.Padding * images.Count) + doc.Padding;
            int height = doc.Direction == Direction.Vertical ? images.Values.Sum(img => img.Height) + (doc.Padding * images.Count) + doc.Padding : images.Values.Max(img => img.Height) + (doc.Padding * 2);

            List<SpriteFragment> fragments = new List<SpriteFragment>();

            using (var bitmap = new Bitmap(width, height))
            {
                bitmap.SetResolution(doc.Resolution, doc.Resolution);

                using (Graphics canvas = Graphics.FromImage(bitmap))
                {
                    if (doc.Direction == Direction.Vertical)
                        Vertical(images, fragments, canvas, doc.Padding);
                    else
                        Horizontal(images, fragments, canvas, doc.Padding);
                }

                string outputFile = Path.ChangeExtension(doc.FileName, doc.OutputExtension);

                OnSaving(outputFile, doc);
                bitmap.Save(outputFile, ImageHelpers.ExtensionFromFormat(doc.Format));
                OnSaved(outputFile, doc);
            }

            await SpriteExporter.ExportStylesheet(fragments, doc, this);
        }

        private static void Vertical(Dictionary<string, Bitmap> images, List<SpriteFragment> fragments, Graphics canvas, int margin)
        {
            int currentY = margin;

            foreach (string file in images.Keys)
            {
                Image img = images[file];
                fragments.Add(new SpriteFragment(file, img.Width, img.Height, margin, currentY));

                canvas.DrawImage(img, margin, currentY);
                currentY += img.Height + margin;
            }
        }

        private static void Horizontal(Dictionary<string, Bitmap> images, List<SpriteFragment> fragments, Graphics canvas, int margin)
        {
            int currentX = margin;

            foreach (string file in images.Keys)
            {
                Image img = images[file];
                fragments.Add(new SpriteFragment(file, img.Width, img.Height, currentX, margin));

                canvas.DrawImage(img, currentX, margin);
                currentX += img.Width + margin;
            }
        }

        private static Dictionary<string, Bitmap> GetImages(SpriteDocument doc)
        {
            Dictionary<string, Bitmap> images = new Dictionary<string, Bitmap>();

            foreach (string file in doc.ToAbsoluteImages())
            {
                if (!File.Exists(file))
                {
                    throw new FileNotFoundException("One or more sprite input files don't exist", file);
                }

                var bitmap = (Bitmap)Image.FromFile(file);
                bitmap.SetResolution(doc.Resolution, doc.Resolution);

                images.Add(file, bitmap);
            }

            return images;
        }

        internal void OnSaving(string fileName, SpriteDocument doc)
        {
            Saving?.Invoke(null, new SpriteImageGenerationEventArgs(fileName, doc));
        }

        internal void OnSaved(string fileName, SpriteDocument doc)
        {
            Saved?.Invoke(null, new SpriteImageGenerationEventArgs(fileName, doc));
        }

        public event SpriteImageGenerationEventHandler Saving;
        public event SpriteImageGenerationEventHandler Saved;
    }
}
