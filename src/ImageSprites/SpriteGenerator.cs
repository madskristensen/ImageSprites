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
            Dictionary<string, Image> images = GetImages(doc);

            int width = doc.Direction == SpriteDirection.Vertical ? images.Values.Max(i => i.Width) + (doc.Padding * 2) : images.Values.Sum(i => i.Width) + (doc.Padding * images.Count) + doc.Padding;
            int height = doc.Direction == SpriteDirection.Vertical ? images.Values.Sum(img => img.Height) + (doc.Padding * images.Count) + doc.Padding : images.Values.Max(img => img.Height) + (doc.Padding * 2);

            List<SpriteFragment> fragments = new List<SpriteFragment>();

            using (var bitmap = new Bitmap(width, height))
            {
                using (Graphics canvas = Graphics.FromImage(bitmap))
                {
                    if (doc.Direction == SpriteDirection.Vertical)
                        Vertical(images, fragments, canvas, doc.Padding);
                    else
                        Horizontal(images, fragments, canvas, doc.Padding);
                }

                string outputFile = Path.ChangeExtension(doc.FileName, doc.OutputExtension);

                OnSaving(outputFile, doc);
                bitmap.Save(outputFile, ImageHelpers.ExtensionFromFormat(doc.Format));
                OnSaved(outputFile, doc);
            }

            await SpriteExporter.ExportStylesheet(fragments, doc);
        }

        private static void Vertical(Dictionary<string, Image> images, List<SpriteFragment> fragments, Graphics canvas, int margin)
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

        private static void Horizontal(Dictionary<string, Image> images, List<SpriteFragment> fragments, Graphics canvas, int margin)
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

        private static Dictionary<string, Image> GetImages(SpriteDocument sprite)
        {
            Dictionary<string, Image> images = new Dictionary<string, Image>();

            foreach (string file in sprite.ToAbsoluteImages())
            {
                if (!File.Exists(file))
                {
                    return null;
                }

                Image image = Image.FromFile(file);

                // Only touch the resolution of the image if it isn't 96.
                // That way we keep the original image 'as is' in all other cases.
                if (Math.Round(image.VerticalResolution) != 96F || Math.Round(image.HorizontalResolution) != 96F)
                    image = new Bitmap(image);

                images.Add(file, image);
            }

            return images;
        }

        private void OnSaving(string fileName, SpriteDocument doc)
        {
            Saving?.Invoke(null, new SpriteImageGenerationEventArgs(fileName, doc));
        }

        private void OnSaved(string fileName, SpriteDocument doc)
        {
            Saved?.Invoke(null, new SpriteImageGenerationEventArgs(fileName, doc));
        }

        public event SpriteImageGenerationEventHandler Saving;
        public event SpriteImageGenerationEventHandler Saved;
    }
}
