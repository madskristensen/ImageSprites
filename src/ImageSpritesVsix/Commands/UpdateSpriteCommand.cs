using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Shell;

namespace ImageSpritesVsix
{
    internal sealed class UpdateSpriteCommand
    {
        private readonly Package _package;
        private static readonly string[] _allowd = { ".png", ".jpg", ".jpeg", ".gif" };

        private UpdateSpriteCommand(Package package)
        {
            _package = package;

            var commandService = (OleMenuCommandService)ServiceProvider.GetService(typeof(IMenuCommandService));

            var id = new CommandID(PackageGuids.guidImageSpriteCmdSet, PackageIds.UpdateSprite);
            var cmd = new OleMenuCommand(Execute, id);
            cmd.BeforeQueryStatus += BeforeQueryStatus;
            commandService.AddCommand(cmd);
        }

        public static UpdateSpriteCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package)
        {
            Instance = new UpdateSpriteCommand(package);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            var files = ProjectHelpers.GetSelectedItemPaths();

            var isSprite = files.Count() == 1 && Path.GetExtension(files.First()) == Constants.FileExtension;

            button.Enabled = button.Visible = isSprite;
        }

        private async void Execute(object sender, EventArgs e)
        {
            var files = ProjectHelpers.GetSelectedItemPaths();
            await SpriteService.GenerateSprite(files.First());
        }
    }
}
