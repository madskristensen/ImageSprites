using System;

namespace ImageSprites
{
    public class SpriteParseException : Exception
    {
        public SpriteParseException(string fileName, Exception innerException)
            : base("The sprite document contains errors that prevents it from generating an image sprite.", innerException)
        {
            FileName = fileName;
        }

        public string FileName { get; }
    }
}
