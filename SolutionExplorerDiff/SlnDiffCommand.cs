// <copyright file="SlnDiffCommand.cs" company="Matt Lacey Ltd">
// Copyright (c) Matt Lacey Ltd. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace SolutionExplorerDiff
{
    internal sealed class SlnDiffCommand
    {
        public const int CommandId = 0x0100;

        public static readonly Guid CommandSet = new Guid("36464c44-64d0-4218-8aea-ddbb991839ac");

        private readonly AsyncPackage package;

        private string filePath1;
        private string filePath2;

        private SlnDiffCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += this.MenuItem_BeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        public static SlnDiffCommand Instance { get; private set; }

        private IAsyncServiceProvider ServiceProvider { get { return this.package; } }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SlnDiffCommand(package, commandService);
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var button = (OleMenuCommand)sender;
            var showButton = false;

            if (await this.ServiceProvider.GetServiceAsync(typeof(DTE)) is DTE2 dte)
            {
                var items = dte.ToolWindows.SolutionExplorer.SelectedItems as UIHierarchyItem[];

                if (items.Length == 2)
                {
                    if (items[0].Object is ProjectItem pi1)
                    {
                        this.filePath1 = pi1.Properties.Item("FullPath").Value.ToString();

                        if (items[1].Object is ProjectItem pi2)
                        {
                            this.filePath2 = pi2.Properties.Item("FullPath").Value.ToString();

                            showButton = true;
                        }
                    }
                }
            }

            button.Visible = showButton;
            button.Enabled = showButton;
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void Execute(object sender, EventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (await this.ServiceProvider.GetServiceAsync(typeof(DTE)) is DTE dte)
            {
                dte.ExecuteCommand("Tools.DiffFiles", $"\"{this.filePath1}\" \"{this.filePath2}\"");
            }
        }
    }
}
