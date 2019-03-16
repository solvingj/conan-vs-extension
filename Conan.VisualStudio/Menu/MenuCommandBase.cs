using System;
using System.ComponentModel.Design;
using Conan.VisualStudio.Services;
using Microsoft.VisualStudio.Shell;

namespace Conan.VisualStudio.Menu
{
    /// <summary>Base class for a menu command.</summary>
    internal abstract class MenuCommandBase
    {
        private static readonly Guid CommandSetId = new Guid("614d6e2d-166a-4d8c-b047-1c2248bbef97");

        private readonly IDialogService _dialogService;
        private readonly OleMenuCommand _menuCommand;

        protected abstract int CommandId { get; }

        public MenuCommandBase(IMenuCommandService commandService, IDialogService dialogService)
        {
            _dialogService = dialogService;
            var menuCommandId = new CommandID(CommandSetId, CommandId);
            _menuCommand = new OleMenuCommand(MenuItemCallback, menuCommandId);
            _menuCommand.BeforeQueryStatus += new EventHandler(OnBeforeQueryStatus);
            commandService.AddCommand(_menuCommand);
        }

        protected internal abstract System.Threading.Tasks.Task MenuItemCallbackAsync();

        async System.Threading.Tasks.Task CallMenuItemBallbackAsync()
        {
            try
            {
                await MenuItemCallbackAsync();
            }
            catch (Exception exception)
            {
                _dialogService.ShowPluginError(exception.ToString());
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(
                async delegate
                {
                    await CallMenuItemBallbackAsync();
                }
            );
        }

        public void EnableMenu(bool enable)
        {
            _menuCommand.Enabled = enable;
        }

        protected virtual void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            // do nothing, override in child, if necessary
        }
    }
}
