using System;

namespace ImageSprites
{
    /// <summary>
    /// EventArgs for sprite image generation.
    /// </summary>
    public class SpriteImageGenerationEventArgs: EventArgs
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public SpriteImageGenerationEventArgs(string fileName, SpriteDocument document)
        {
            FileName = fileName;
            Document = document;
        }

        /// <summary>The name of the file being generated.</summary>
        public string FileName { get; set; }

        /// <summary>The <see cref="SpriteDocument"/> used in the generation.</summary>
        public SpriteDocument Document { get; set; }
    }
}
