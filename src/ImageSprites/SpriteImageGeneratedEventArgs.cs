using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSprites
{
    public class SpriteImageGenerationEventArgs: EventArgs
    {
        public SpriteImageGenerationEventArgs(string fileName, SpriteDocument document)
        {
            FileName = fileName;
            Document = document;
        }

        public string FileName { get; set; }
        public SpriteDocument Document { get; set; }
    }

    public delegate void SpriteImageGenerationEventHandler(object sender, SpriteImageGenerationEventArgs e);
}
