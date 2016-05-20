using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace ImageSpritesVsix
{
    class SpriteContentTypeDefinition
    {
        public const string SpriteContentType = "Sprite";

        [Export(typeof(ContentTypeDefinition))]
        [Name(SpriteContentType)]
        [BaseDefinition("json")]
        public ContentTypeDefinition ISpriteContentTypeDefinitionContentType { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(SpriteContentType)]
        [FileExtension(Constants.FileExtension)]
        public FileExtensionToContentTypeDefinition SpriteFileExtensionDefinition { get; set; }
    }
}
