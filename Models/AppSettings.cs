using System.Collections.Generic;

namespace SleepOnLan.Models
{
    public class AppSettings
    {
        public int DefaultPort { get; set; } = 9999;
        public string ScreenshotSavePath { get; set; } = "";
        public List<AppInfo> RegisteredApps { get; set; } = new();
    }

    public class AppInfo
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
    }
}
