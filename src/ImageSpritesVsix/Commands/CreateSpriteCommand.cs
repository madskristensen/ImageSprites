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

        public static async System.Threading.Tasks.Task Initialize(AsyncPackage package)
        {
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CreateSpriteCommand(commandService);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            var files = GetFiles();

            button.Enabled = button.Visible = files.Any();
        }

        private async void Execute(object sender, EventArgs e)
        {
            var files = GetFiles();
            var folder = Path.GetDirectoryName(files.First());
            string spriteFile;

            if (GetFileName(folder, out spriteFile))
            {
                var doc = new SpriteDocument(spriteFile, files);
                doc.Stylesheet = Stylesheet.Css;

                await doc.Save();
                ProjectHelpers.DTE.ItemOperations.OpenFile(doc.FileName);
                await SpriteService.GenerateSprite(doc);
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
                    return false;

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
