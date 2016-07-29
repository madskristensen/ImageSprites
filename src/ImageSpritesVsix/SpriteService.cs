using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageSprites;

namespace ImageSpritesVsix
{
    class SpriteService
    {
        private static SpriteImageGenerator _generator;

        public static void Initialize()
        {
            SpriteDocument.Saving += SpriteSaving;
            SpriteDocument.Saved += SpriteSaved;

            _generator = new SpriteImageGenerator();
            _generator.Saving += SpriteImageSaving;
            _generator.Saved += SpriteImageSaved;
        }

        private static async void SpriteImageSaved(object sender, SpriteImageGenerationEventArgs e)
        {
            ProjectHelpers.AddNestedFile(e.Document.FileName, e.FileName);

            var project = ProjectHelpers.DTE.Solution.FindProjectItem(e.Document.FileName)?.ContainingProject;

            if (project != null && project.IsKind(ProjectHelpers.ProjectTypes.ASPNET_5))
                await Task.Delay(2000);

            OptimizeImage(e.FileName, e.Document.Optimize);
        }

        private static void OptimizeImage(string fileName, Optimization optimization)
        {
            try
            {
                string ext = Path.GetExtension(fileName);

                if (optimization != Optimization.None && Constants.SupporedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                {
                    ProjectHelpers.SelectInSolutionExplorer(fileName);
                    string cmd = "ProjectandSolutionContextMenus.Project.ImageOptimizer.OptimzeImagelossless";

                    if (optimization == Optimization.Lossy)
                        cmd = "ProjectandSolutionContextMenus.Project.ImageOptimizer.OptimzeImagelossy";

                    ProjectHelpers.ExecuteCommand(cmd);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private static void SpriteImageSaving(object sender, SpriteImageGenerationEventArgs e)
        {
            ProjectHelpers.CheckFileOutOfSourceControl(e.FileName);
        }

        private static void SpriteSaved(object sender, FileSystemEventArgs e)
        {
            var project = ProjectHelpers.GetActiveProject();

            if (project != null)
            {
                ProjectHelpers.AddFileToProject(project, e.FullPath, "None");
            }
        }

        private static void SpriteSaving(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                ProjectHelpers.CheckFileOutOfSourceControl(e.FullPath);
            }
        }

        public static async Task GenerateSprite(string fileName)
        {
            try
            {
                var doc = await SpriteDocument.FromFile(fileName);
                await GenerateSprite(doc);
            }
            catch (SpriteParseException ex)
            {
                MessageBox.Show(ex.Message, Vsix.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                ProjectHelpers.DTE.ItemOperations.OpenFile(fileName);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public static async Task GenerateSprite(SpriteDocument doc)
        {
            try
            {
                await _generator.Generate(doc);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message, Vsix.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                ProjectHelpers.DTE.ItemOperations.OpenFile(doc.FileName);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
