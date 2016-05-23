using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace ImageSpritesVsix
{
    internal sealed class UpdateAllSpritesCommand
    {
        private readonly Package _package;
        private Project _project;
        private Solution2 _solution;

        private UpdateAllSpritesCommand(Package package)
        {
            _package = package;

            var commandService = (OleMenuCommandService)ServiceProvider.GetService(typeof(IMenuCommandService));

            var id = new CommandID(PackageGuids.guidImageSpriteCmdSet, PackageIds.UpdateAllSprite);
            var cmd = new OleMenuCommand(Execute, id);
            cmd.BeforeQueryStatus += BeforeQueryStatus;
            commandService.AddCommand(cmd);
        }

        public static UpdateAllSpritesCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package)
        {
            Instance = new UpdateAllSpritesCommand(package);
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

            var files = GetFiles(folder, "*.sprite");

            foreach (var file in files)
            {
                await SpriteService.GenerateSprite(file);
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
                foreach (var directory in Directory.GetDirectories(path))
                    files.AddRange(GetFiles(directory, pattern));
            }
            catch { }

            return files;
        }
    }
}
