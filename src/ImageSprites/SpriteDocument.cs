using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ImageSprites
{
    public class SpriteDocument
    {
        public SpriteDocument()
        { }

        public SpriteDocument(string fileName)
            : this(fileName, Enumerable.Empty<string>())
        { }

        public SpriteDocument(string fileName, IEnumerable<string> images)
        {
            FileName = fileName;
            Images = images;

            if (images.Any())
            {
                var first = Image.FromFile(images.First());
                Resolution = (int)Math.Round(first.HorizontalResolution);

                Format = ImageHelpers.GetImageFormatFromExtension(images.First());
            }
        }

        [JsonIgnore]
        public string FileName { get; set; }

        [JsonProperty("images")]
        public IEnumerable<string> Images { get; set; }

        [JsonProperty("direction")]
        public Direction Direction { get; set; } = Direction.Vertical;

        [JsonProperty("optimize")]
        public Optimizations Optimize { get; set; } = Optimizations.Lossless;

        [JsonProperty("padding")]
        public int Padding { get; set; } = 10;

        [JsonProperty("format")]
        public ImageType Format { get; set; } = ImageType.Png;

        [JsonProperty("stylesheets")]
        public Stylesheet Stylesheets { get; set; }

        [JsonProperty("dpi")]
        public int Resolution { get; set; } = 96;

        [JsonIgnore]
        public string OutputExtension
        {
            get
            {
                return "." + Format.ToString().ToLowerInvariant();
            }
        }

        public static async Task<SpriteDocument> FromFile(string fileName)
        {
            try
            {
                using (var reader = new StreamReader(fileName))
                {
                    var content = await reader.ReadToEndAsync().ConfigureAwait(false);
                    var doc = JsonConvert.DeserializeObject<SpriteDocument>(content);

                    doc.FileName = fileName;

                    return doc;
                }
            }
            catch (JsonSerializationException ex)
            {
                throw new SpriteParseException(fileName, ex);
            }
        }

        public IEnumerable<string> ToAbsoluteImages()
        {
            var dir = Path.GetDirectoryName(FileName);

            foreach (var file in Images)
            {
                yield return new FileInfo(Path.Combine(dir, file)).FullName;
            }
        }

        public async Task Save()
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

            var json = JsonConvert.SerializeObject(this, settings);

            using (var writer = new StreamWriter(FileName))
            {
                OnSaving(FileName);
                await writer.WriteAsync(json).ConfigureAwait(false);
                OnSaved(FileName);
            }
        }

        internal void OnSaving(string fileName)
        {
            if (Saving != null)
            {
                var type = File.Exists(fileName) ? WatcherChangeTypes.Changed : WatcherChangeTypes.Created;
                var dir = Path.GetDirectoryName(fileName);
                var name = Path.GetFileName(fileName);

                Saving(this, new FileSystemEventArgs(type, dir, name));
            }
        }

        internal void OnSaved(string fileName)
        {
            if (Saved != null)
            {
                var type = File.Exists(fileName) ? WatcherChangeTypes.Changed : WatcherChangeTypes.Created;
                var dir = Path.GetDirectoryName(fileName);
                var name = Path.GetFileName(fileName);

                Saved(this, new FileSystemEventArgs(type, dir, name));
            }
        }

        public static event FileSystemEventHandler Saving;
        public static event FileSystemEventHandler Saved;
    }
}
