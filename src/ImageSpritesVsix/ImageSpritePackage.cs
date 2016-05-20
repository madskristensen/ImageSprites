using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace ImageSpritesVsix
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidPackageString)]
    public sealed class ImageSpritePackage : Package
    {
        protected override void Initialize()
        {
            CreateSpriteCommand.Initialize(this);
            base.Initialize();
        }
    }
}
