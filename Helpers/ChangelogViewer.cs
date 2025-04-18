using System.IO;
using System.Reflection;
using System.Windows;

namespace WormsDirectManagement.Helpers
{
    internal static class LogViewer
    {
        public static void Show()
        {
            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var path = Path.Combine(exeDir, "invoice_downloader.log");
            MessageBox.Show(File.Exists(path) ? File.ReadAllText(path) : "No logs yet.",
                            "Logs", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static class ChangelogViewer
    {
        public static void Show()
            => MessageBox.Show(Constants.Changelog,
                               "Changelog", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
