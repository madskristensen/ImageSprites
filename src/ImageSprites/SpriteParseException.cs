using System;
using System.Runtime.Serialization;

namespace ImageSprites
{
    /// <summary>
    /// An exception type for JSON sprite manifest syntax errors.
    /// </summary>
    [Serializable]
    public class SpriteParseException : Exception
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public SpriteParseException()
            : base()
        { }

        /// <summary>
        /// Creates an new instance and sets the FileName property.
        /// </summary>
        public SpriteParseException(string FileName)
            : this(FileName, null)
        { }

        /// <summary>
        /// Creates an new instance and sets the FileName and InnerException properties.
        /// </summary>
        public SpriteParseException(string fileName, Exception innerException)
            : base("The sprite document contains errors that prevents it from generating an image sprite.", innerException)
        {
            FileName = fileName;
        }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        protected SpriteParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
            {
                FileName = info.GetString(nameof(FileName));
            }
        }

        /// <summary>
        /// The name of the file containing the syntax error.
        /// </summary>
        public string FileName { get; }

        /// <summary>Serialization specific</summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                info.AddValue(nameof(FileName), FileName);
            }

            base.GetObjectData(info, context);
        }
    }
}
