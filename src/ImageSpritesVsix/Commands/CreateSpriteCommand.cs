using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ImageSprites;
using Microsoft.VisualStudio.Shell;

namespace ImageSpritesVsix
{
    internal sealed class CreateSpriteCommand
    {
        private CreateSpriteCommand(OleMenuCommandService commandService)
        {
            var id = new CommandID(PackageGuids.guidImageSpriteCmdSet, PackageIds.CreateSprite);
            var cmd = new OleMenuCommand(Execute, id);
            cmd.BeforeQueryStatus += BeforeQueryStatus;
            commandService.AddCommand(cmd);
        }

        public static CreateSpriteCommand Instance { get; private set; }

        public static void Initialize(OleMenuCommandService commandService)
        {
            Instance = new CreateSpriteCommand(commandService);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            IEnumerable<string> files = GetFiles();

            button.Enabled = button.Visible = files.Any();
        }

        private void Execute(object sender, EventArgs e)
        {
            IEnumerable<string> files = GetFiles();
            string folder = Path.GetDirectoryName(files.First());

            if (GetFileName(folder, out string spriteFile))
            {
                var doc = new SpriteDocument(spriteFile, files)
                {
                    Stylesheet = Stylesheet.Css
                };

                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    await doc.Save();
                    ProjectHelpers.DTE.ItemOperations.OpenFile(doc.FileName);
                    await SpriteService.GenerateSpriteAsync(doc);
                });
            }
        }

        private bool GetFileName(string initialDirectory, out string fileName)
        {
            fileName = null;

            using (var dialog = new SaveFileDialog())
            {
                dialog.InitialDirectory = initialDirectory;
                dialog.FileName = "mysprite" + Constants.FileExtension;
                dialog.DefaultExt = Constants.FileExtension;
                dialog.Filter = "Sprite files | *" + Constants.FileExtension;

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }

                fileName = dialog.FileName;
            }

            return true;
        }

        private IEnumerable<string> GetFiles()
        {
            return ProjectHelpers.GetSelectedItemPaths().Where(file => Constants.SupporedExtensions.Contains(Path.GetExtension(file)));
        }
    }
}
