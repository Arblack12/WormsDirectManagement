using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace WormsDirectManagement.Helpers
{
    internal static class IconLoader
    {
        public static Icon Load()
        {
            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var path = Path.Combine(exeDir, "Assets", "icon.png");
            return File.Exists(path)
                ? Icon.FromHandle(new Bitmap(path).GetHicon())
                : System.Drawing.SystemIcons.Application;
        }
    }
}
