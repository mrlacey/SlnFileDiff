// <copyright file="SolutionExplorerDiffPackage.cs" company="Matt Lacey Ltd">
// Copyright (c) Matt Lacey Ltd. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace SolutionExplorerDiff
{
    [ProvideAutoLoad(UIContextGuids.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(SolutionExplorerDiffPackage.PackageGuidString)]
    [InstalledProductRegistration("Solution File Diff", "Diff files from the Solution Explorer", "1.0")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class SolutionExplorerDiffPackage : AsyncPackage
    {
        public const string PackageGuidString = "5afff942-f487-4ba1-a4ec-b0f0d2f3a2cd";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await SlnDiffCommand.InitializeAsync(this);
        }
    }
}
