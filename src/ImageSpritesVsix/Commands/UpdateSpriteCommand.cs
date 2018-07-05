using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Shell;

namespace ImageSpritesVsix
{
    internal sealed class UpdateSpriteCommand
    {
        private static readonly string[] _allowd = { ".png", ".jpg", ".jpeg", ".gif" };

        private UpdateSpriteCommand(OleMenuCommandService commandService)
        {
            var id = new CommandID(PackageGuids.guidImageSpriteCmdSet, PackageIds.UpdateSprite);
            var cmd = new OleMenuCommand(Execute, id);
            cmd.BeforeQueryStatus += BeforeQueryStatus;
            commandService.AddCommand(cmd);
        }

        public static UpdateSpriteCommand Instance { get; private set; }

        public static void Initialize(OleMenuCommandService commandService)
        {
            Instance = new UpdateSpriteCommand(commandService);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            System.Collections.Generic.IEnumerable<string> files = ProjectHelpers.GetSelectedItemPaths();

            bool isSprite = files.Count() == 1 && Path.GetExtension(files.First()).Equals(Constants.FileExtension, StringComparison.OrdinalIgnoreCase);

            button.Enabled = button.Visible = isSprite;
        }

        private async void Execute(object sender, EventArgs e)
        {
            System.Collections.Generic.IEnumerable<string> files = ProjectHelpers.GetSelectedItemPaths();
            await SpriteService.GenerateSpriteAsync(files.First());
        }
    }
}
