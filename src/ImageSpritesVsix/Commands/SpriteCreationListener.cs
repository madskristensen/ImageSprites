using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace ImageSpritesVsix
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(SpriteContentTypeDefinition.SpriteContentType)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    class SpriteCreationListener : IVsTextViewCreationListener
    {
        private static bool _hasReloadedSchemas;

        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            ITextDocument doc;

            if (TextDocumentFactoryService.TryGetTextDocument(textView.TextDataModel.DocumentBuffer, out doc))
            {
                doc.FileActionOccurred += DocumentSaved;

                if (!_hasReloadedSchemas)
                {
                    ProjectHelpers.ExecuteCommand("OtherContextMenus.JSONContext.ReloadSchemas");
                    _hasReloadedSchemas = true;
                }
            }
        }

        private void DocumentSaved(object sender, TextDocumentFileActionEventArgs e)
        {
            if (e.FileActionType == FileActionTypes.ContentSavedToDisk)
            {
                SpriteService.GenerateSpriteAsync(e.FilePath).ConfigureAwait(false);
            }
        }
    }
}
