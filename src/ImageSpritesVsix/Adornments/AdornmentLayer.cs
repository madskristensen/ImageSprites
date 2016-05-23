using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace ImageSpritesVsix
{
    class AdornmentLayer
    {
        public const string LayerName = "Sprite Logo";

        [Export(typeof(AdornmentLayerDefinition))]
        [Name(LayerName)]
        [Order(Before = PredefinedAdornmentLayers.Caret)]
        public AdornmentLayerDefinition editorAdornmentLayer = null;
    }
}
