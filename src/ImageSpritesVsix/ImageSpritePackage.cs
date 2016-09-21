using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using task = System.Threading.Tasks.Task;

namespace ImageSpritesVsix
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuids.guidPackageString)]
    internal sealed class ImageSpritePackage : AsyncPackage
    {
        protected override async task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await Logger.InitializeAsync(this, Vsix.Name);
            await SpriteService.Initialize();

            var commandService = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

            await JoinableTaskFactory.SwitchToMainThreadAsync();

            CreateSpriteCommand.Initialize(commandService);
            UpdateSpriteCommand.Initialize(commandService);
            UpdateAllSpritesCommand.Initialize(commandService);
        }
    }
}
