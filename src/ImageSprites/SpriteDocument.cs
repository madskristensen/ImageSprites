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
            AddImages(images);

            if (images.Any())
            {
                var first = Image.FromFile(images.First());
                Resolution = (int)Math.Round(first.HorizontalResolution);

                Format = SpriteHelpers.GetImageFormatFromExtension(images.First());
            }
        }

        [JsonIgnore]
        public string FileName { get; set; }

        [JsonProperty("images")]
        public IDictionary<string, string> Images { get; set; }

        [JsonProperty("direction")]
        public Direction Direction { get; set; } = Direction.Vertical;

        [JsonProperty("optimize")]
        public Optimizations Optimize { get; set; } = Optimizations.Lossless;

        [JsonProperty("padding")]
        public int Padding { get; set; } = 10;

        [JsonProperty("format")]
        public ImageType Format { get; set; } = ImageType.Png;

        [JsonProperty("stylesheets")]
        public Stylesheet Stylesheets { get; set; } = new Stylesheet();

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
            using (var reader = new StreamReader(fileName))
            {
                var content = await reader.ReadToEndAsync().ConfigureAwait(false);
                var doc = FromJSON(content, fileName);

                doc.FileName = fileName;

                return doc;
            }
        }

        public static SpriteDocument FromJSON(string json, string fileName = null)
        {
            try
            {
                return JsonConvert.DeserializeObject<SpriteDocument>(json);
            }
            catch (JsonSerializationException ex)
            {
                throw new SpriteParseException(fileName, ex);
            }
        }

        public IDictionary<string, string> ToAbsoluteImages()
        {
            var dir = Path.GetDirectoryName(FileName);
            var dic = new Dictionary<string, string>();

            foreach (var ident in Images.Keys)
            {
                dic.Add(ident, new FileInfo(Path.Combine(dir, Images[ident])).FullName);
            }

            return dic;
        }

        public string ToJsonString()
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

            return JsonConvert.SerializeObject(this, settings);
        }

        public async Task Save()
        {
            var json = ToJsonString();

            using (var writer = new StreamWriter(FileName))
            {
                OnSaving(FileName);
                await writer.WriteAsync(json).ConfigureAwait(false);
                OnSaved(FileName);
            }
        }

        public void AddImages(IEnumerable<string> files)
        {
            var dic = new Dictionary<string, string>();

            foreach (var file in files)
            {
                string name = SpriteHelpers.GetIdentifier(file);

                if (dic.ContainsKey(name))
                    name += "_" + Guid.NewGuid().ToString().Replace("-", string.Empty);

                dic.Add(name, file);
            }

            Images = dic;
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
