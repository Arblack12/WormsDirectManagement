using System.Diagnostics;
using System.IO;

namespace WormsDirectManagement.Helpers
{
    internal static class Paths
    {
        public static void OpenFolder(string folder)
        {
            if (Directory.Exists(folder))
                Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
        }
    }
}
