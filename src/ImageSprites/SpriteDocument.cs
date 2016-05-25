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
    /// <summary>
    /// The sprite manifest document containing all information needed to produce the image sprite.
    /// </summary>
    public class SpriteDocument
    {
        /// <summary>Creates a new instance with default values.</summary>
        public SpriteDocument()
        { }

        /// <summary>Creates a new instance and calculates values based in provided images.</summary>
        public SpriteDocument(string fileName, IEnumerable<string> images)
        {
            FileName = fileName;
            AddImages(images);

            if (images.Any())
            {
                var first = Image.FromFile(images.First());
                Dpi = (int)Math.Round(first.HorizontalResolution);
                Output = SpriteHelpers.GetImageFormatFromExtension(images.First());
            }
        }

        /// <summary>The absolute file name.</summary>
        [JsonIgnore]
        public string FileName { get; private set; }

        /// <summary>The individual images that makes up the sprite.</summary>
        [JsonProperty("images")]
        public IDictionary<string, string> Images { get; private set; }

        /// <summary>The orientation of the individual images inside the sprite.</summary>
        [JsonProperty("orientation")]
        public Orientation Orientation { get; set; } = Orientation.Vertical;

        /// <summary>Image optimization settings.</summary>
        [JsonProperty("optimize")]
        public Optimizations Optimize { get; set; } = Optimizations.Lossless;

        /// <summary>The padding size in pixels around each individual image in the sprite.</summary>
        [JsonProperty("padding")]
        public int Padding { get; set; } = 10;

        /// <summary>The output format of the generated sprite image.</summary>
        [JsonProperty("output")]
        public ImageType Output { get; set; } = ImageType.Png;

        /// <summary>The DPI of the generated sprite image.</summary>
        [JsonProperty("dpi")]
        public int Dpi { get; set; } = 96;

        /// <summary>The type of stylesheet to generate.</summary>
        [JsonProperty("stylesheet")]
        public Stylesheet Stylesheet { get; set; } = Stylesheet.None;

        /// <summary>The path to prepend to url in the stylesheet's "url()" function.</summary>
        [JsonProperty("pathprefix")]
        public string PathPrefix { get; set; } = string.Empty;

        /// <summary>The path to prepend to url in the stylesheet's "url()" function.</summary>
        [JsonProperty("customstyles")]
        public IDictionary<string, object> CustomStyles { get; set; } = new Dictionary<string, object> { { "display", "block" } };

        /// <summary>The file extension of the output sprite image.</summary>
        [JsonIgnore]
        public string OutputExtension
        {
            get
            {
                return "." + Output.ToString().ToLowerInvariant();
            }
        }

        /// <summary>
        /// Create an instance from a .sprite file.
        /// </summary>
        /// <param name="fileName">A valid .sprite JSON file.</param>
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

        /// <summary>
        /// Creates an instance from the specified JSON string.
        /// </summary>
        /// <param name="json">A string of valid JSON.</param>
        /// <param name="fileName">Optionally provide a file name for error handling purposes.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Converts the SpriteDocument to its JSON string representation.
        /// </summary>
        /// <returns>A JSON string representation of SpriteDocument instance.</returns>
        public string ToJsonString()
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

            return JsonConvert.SerializeObject(this, settings);
        }

        /// <summary>
        /// Add image files to the sprite.
        /// </summary>
        /// <param name="files">A list of relative file paths.</param>
        private void AddImages(IEnumerable<string> files)
        {
            var dic = new Dictionary<string, string>();

            foreach (var file in files)
            {
                string name = SpriteHelpers.GetIdentifier(file);
                string relative = SpriteHelpers.MakeRelative(FileName, file);

                if (dic.ContainsKey(name))
                    name += "_" + Guid.NewGuid().ToString().Replace("-", string.Empty);

                dic.Add(name, relative);
            }

            Images = dic;
        }

        /// <summary>
        /// Saves the SpriteDocument as a JSON file to the FileName location.
        /// </summary>
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

        internal IDictionary<string, string> ToAbsoluteImages()
        {
            var dir = Path.GetDirectoryName(FileName);
            var dic = new Dictionary<string, string>();

            foreach (var ident in Images.Keys)
            {
                dic.Add(ident, new FileInfo(Path.Combine(dir, Images[ident])).FullName);
            }

            return dic;
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

        /// <summary>Fires before a file is written to disk.</summary>
        public static event FileSystemEventHandler Saving;

        /// <summary>Fires after a file is written to disk.</summary>
        public static event FileSystemEventHandler Saved;

        /// <summary>Internal use only. Used by the JSON.NET serializer</summary>
        public bool ShouldSerializeDpi()
        {
            return Dpi != 96;
        }

        /// <summary>Internal use only. Used by the JSON.NET serializer</summary>
        public bool ShouldSerializeStylesheet()
        {
            return Stylesheet != Stylesheet.None;
        }
    }
}
