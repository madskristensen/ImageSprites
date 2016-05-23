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
        private readonly Package _package;

        private CreateSpriteCommand(Package package)
        {
            _package = package;

            var commandService = (OleMenuCommandService)ServiceProvider.GetService(typeof(IMenuCommandService));

            var id = new CommandID(PackageGuids.guidImageSpriteCmdSet, PackageIds.CreateSprite);
            var cmd = new OleMenuCommand(Execute, id);
            cmd.BeforeQueryStatus += BeforeQueryStatus;
            commandService.AddCommand(cmd);
        }

        public static CreateSpriteCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package)
        {
            Instance = new CreateSpriteCommand(package);
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
                var doc = new SpriteDocument(spriteFile);
                doc.AddImages(files.Select(f => SpriteHelpers.MakeRelative(spriteFile, f)));

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
