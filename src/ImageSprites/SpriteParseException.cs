using System;

namespace ImageSprites
{
    /// <summary>
    /// An exception type for JSON sprite manifest syntax errors.
    /// </summary>
    public class SpriteParseException : Exception
    {
        /// <summary>
        /// Creates an new instance and sets the FileName property.
        /// </summary>
        public SpriteParseException(string fileName, Exception innerException)
            : base("The sprite document contains errors that prevents it from generating an image sprite.", innerException)
        {
            FileName = fileName;
        }

        /// <summary>
        /// The name of the file containing the syntax error.
        /// </summary>
        public string FileName { get; }
    }
}
