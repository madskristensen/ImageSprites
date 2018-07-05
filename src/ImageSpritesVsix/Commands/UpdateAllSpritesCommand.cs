using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace ImageSpritesVsix
{
    internal sealed class UpdateAllSpritesCommand
    {
        private Project _project;
        private Solution2 _solution;

        private UpdateAllSpritesCommand(OleMenuCommandService commandService)
        {
            var id = new CommandID(PackageGuids.guidImageSpriteCmdSet, PackageIds.UpdateAllSprite);
            var cmd = new OleMenuCommand(Execute, id);
            cmd.BeforeQueryStatus += BeforeQueryStatus;
            commandService.AddCommand(cmd);
        }

        public static UpdateAllSpritesCommand Instance { get; private set; }

        public static void Initialize(OleMenuCommandService commandService)
        {
            Instance = new UpdateAllSpritesCommand(commandService);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;

            button.Enabled = button.Visible = ProjectHelpers.GetProjectOrSolution(out _project, out _solution);
        }

        private async void Execute(object sender, EventArgs e)
        {
            string folder = _project != null ? _project.GetRootFolder() : Path.GetDirectoryName(_solution.FileName);

            if (!Directory.Exists(folder))
                return;

            List<string> files = GetFiles(folder,  "*" + Constants.FileExtension);

            foreach (string file in files)
            {
                await SpriteService.GenerateSpriteAsync(file);
            }
        }

        private static List<string> GetFiles(string path, string pattern)
        {
            var files = new List<string>();

            if (path.Contains("node_modules"))
                return files;

            try
            {
                files.AddRange(Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly));
                foreach (string directory in Directory.GetDirectories(path))
                    files.AddRange(GetFiles(directory, pattern));
            }
            catch { }

            return files;
        }
    }
}
