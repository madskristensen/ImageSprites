using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace ImageSpritesVsix
{
    class SpriteContentTypeDefinition
    {
        public const string SpriteContentType = "Sprite";
        private const string SpriteFileExtension = ".Sprite";

        [Export(typeof(ContentTypeDefinition))]
        [Name(SpriteContentType)]
        [BaseDefinition("json")]
        public ContentTypeDefinition ISpriteContentTypeDefinitionContentType { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(SpriteContentType)]
        [FileExtension(SpriteFileExtension)]
        public FileExtensionToContentTypeDefinition SpriteFileExtensionDefinition { get; set; }
    }
}
