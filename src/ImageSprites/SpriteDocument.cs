using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ImageSprites
{
    public class SpriteDocument
    {
        public SpriteDocument(string fileName)
        {
            FileName = fileName;
        }

        [JsonIgnore]
        public string FileName { get; set; }

        [JsonProperty("images")]
        public IEnumerable<string> Images { get; set; }

        [JsonProperty("direction")]
        public SpriteDirection Direction { get; set; } = SpriteDirection.Vertical;

        [JsonProperty("padding")]
        public int Padding { get; set; } = 10;

        [JsonProperty("format")]
        public ImageType Format { get; set; } = ImageType.Png;

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
                var doc = JsonConvert.DeserializeObject<SpriteDocument>(content);

                doc.FileName = fileName;

                return doc;
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
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);

            using (var writer = new StreamWriter(FileName))
            {
                FileEvents.OnSaving(FileName);
                await writer.WriteAsync(json).ConfigureAwait(false);
                FileEvents.OnSaved(FileName);
            }
        }
    }
}
