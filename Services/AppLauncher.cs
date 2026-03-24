using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SleepOnLan.Services
{
    public class AppLauncher
    {
        private readonly Dictionary<string, string> _appPaths = new();

        public void LoadApps(IEnumerable<Models.AppInfo> apps)
        {
            _appPaths.Clear();
            foreach (var app in apps)
            {
                if (!string.IsNullOrEmpty(app.Name) && !string.IsNullOrEmpty(app.Path))
                {
                    _appPaths[app.Name.ToLower()] = app.Path;
                }
            }
        }

        public (bool Success, string Message) Launch(string appName)
        {
            string key = appName.ToLower();
            
            if (!_appPaths.TryGetValue(key, out string? appPath))
            {
                return (false, $"未知应用: {appName}");
            }

            if (!File.Exists(appPath))
            {
                return (false, $"应用路径不存在: {appPath}");
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = appPath,
                    UseShellExecute = true
                });
                return (true, $"已启动应用: {appName}");
            }
            catch (System.Exception ex)
            {
                return (false, $"启动应用失败: {ex.Message}");
            }
        }

        public void RegisterApp(string name, string path)
        {
            _appPaths[name.ToLower()] = path;
        }

        public IEnumerable<string> GetRegisteredApps()
        {
            return _appPaths.Keys;
        }
    }
}
