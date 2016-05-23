using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using ImageSprites;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.DragDrop;

namespace ImageSpritesVsix
{
    internal class IgnoreDropHandler : IDropHandler
    {
        private IWpfTextView _view;
        private string _draggedFileName;
        private string _documentFileName;

        public IgnoreDropHandler(IWpfTextView view, string fileName)
        {
            _view = view;
            _documentFileName = fileName;
        }

        public DragDropPointerEffects HandleDataDropped(DragDropInfo dragDropInfo)
        {
            var position = dragDropInfo.VirtualBufferPosition.Position;
            var doc = SpriteDocument.FromJSON(_view.TextBuffer.CurrentSnapshot.GetText());

            string ident = SpriteHelpers.GetIdentifier(_draggedFileName);
            string file = SpriteHelpers.MakeRelative(_documentFileName, _draggedFileName);

            if (doc.Images.ContainsKey(ident))
                ident += "_" + Guid.NewGuid().ToString().Replace("-", string.Empty);

            doc.Images.Add(new KeyValuePair<string, string>(ident, file));

            using (var edit = _view.TextBuffer.CreateEdit())
            {
                edit.Replace(0, _view.TextBuffer.CurrentSnapshot.Length, doc.ToJsonString());
                edit.Apply();
            }

            return DragDropPointerEffects.Copy;
        }

        public void HandleDragCanceled()
        { }

        public DragDropPointerEffects HandleDragStarted(DragDropInfo dragDropInfo)
        {
            return DragDropPointerEffects.All;
        }

        public DragDropPointerEffects HandleDraggingOver(DragDropInfo dragDropInfo)
        {
            return DragDropPointerEffects.All;
        }

        public bool IsDropEnabled(DragDropInfo dragDropInfo)
        {
            _draggedFileName = GetImageFilename(dragDropInfo);

            return File.Exists(_draggedFileName);
        }

        private static string GetImageFilename(DragDropInfo info)
        {
            var data = new DataObject(info.Data);

            if (info.Data.GetDataPresent("FileDrop"))
            {
                // The drag and drop operation came from the file system
                var files = data.GetFileDropList();

                if (files != null && files.Count == 1)
                {
                    return files[0];
                }
            }
            else if (info.Data.GetDataPresent("CF_VSSTGPROJECTITEMS"))
            {
                // The drag and drop operation came from the VS solution explorer
                return data.GetText();
            }

            return null;
        }
    }
}