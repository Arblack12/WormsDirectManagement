using System;
using System.Windows;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using WormsDirectManagement.Services;

namespace WormsDirectManagement
{
    public partial class App : Application
    {
        private NotifyIcon? _tray;
        private MainWindow? _window;
        private EmailAttachmentService? _service;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1️⃣  read config
            var config = Helpers.IniConfig.Load();

            // 2️⃣  spin up core service
            _service = new EmailAttachmentService(config);

            // 3️⃣  main window
            _window = new MainWindow(_service);

            // 4️⃣  tray icon
            _tray = new NotifyIcon
            {
                Icon = Helpers.IconLoader.Load(),
                Visible = true,
                Text = $"Worms Direct Management v{Constants.Version}"
            };
            _tray.DoubleClick += (_, __) => ToggleMainWindow();

            BuildTrayMenu();

            // start polling immediately
            _service.CheckEmailsAsync();
        }

        private void BuildTrayMenu()
        {
            var menu = new ContextMenuStrip();

            void Add(string text, Action action)
            {
                var mi = new ToolStripMenuItem(text);
                mi.Click += (_, __) => action();
                menu.Items.Add(mi);
            }

            Add("Show Main Window", () => ToggleMainWindow());
            Add("Check Now", () => _ = _service!.CheckEmailsAsync());
            Add("Pause Monitoring", () => _service!.Pause());
            Add("Resume Monitoring", () => _service!.Resume());
            Add("View Logs", () => Helpers.LogViewer.Show());
            Add("View Changelog", () => Helpers.ChangelogViewer.Show());
            Add("Open Download Folder", () => Helpers.Paths.OpenFolder(_service!.DownloadFolder));
            menu.Items.Add(new ToolStripSeparator());
            Add("Exit", () =>
            {
                _tray!.Visible = false;
                Shutdown();
            });

            _tray!.ContextMenuStrip = menu;
        }

        private void ToggleMainWindow()
        {
            if (_window!.IsVisible)
            {
                _window.Hide();
            }
            else
            {
                _window.Show();
                _window.Activate();
            }
        }
    }
}
