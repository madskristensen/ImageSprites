using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ImageSpritesVsix
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [Guid(PackageGuids.guidPackageString)]
    public sealed class ImageSpritePackage : Package
    {
        protected override void Initialize()
        {
            Logger.Initialize(this, Vsix.Name);

            SpriteService.Initialize();
            CreateSpriteCommand.Initialize(this);
            UpdateSpriteCommand.Initialize(this);
            UpdateAllSpritesCommand.Initialize(this);

            base.Initialize();
        }
    }
}
