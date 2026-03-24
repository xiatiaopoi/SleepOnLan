using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SleepOnLan.Models;

namespace SleepOnLan.Services
{
    public class CommandHandler
    {
        private readonly ScreenshotService _screenshotService;
        private readonly AppLauncher _appLauncher;
        private readonly SettingsService _settingsService;

        [DllImport("Powrprof.dll", SetLastError = true)]
        private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        public CommandHandler(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _screenshotService = new ScreenshotService();
            _appLauncher = new AppLauncher();
            
            LoadAppsFromSettings();
        }

        private void LoadAppsFromSettings()
        {
            _appLauncher.LoadApps(_settingsService.Settings.RegisteredApps);
        }

        public void ReloadApps()
        {
            LoadAppsFromSettings();
        }

        public async Task<CommandResult> HandleAsync(string command)
        {
            string cmdLower = command.ToLower().Trim();

            if (cmdLower == "sleep")
            {
                return HandleSleep();
            }
            
            if (cmdLower == "shutdown")
            {
                return HandleShutdown();
            }
            
            if (cmdLower == "status")
            {
                return HandleStatus();
            }
            
            if (cmdLower.StartsWith("app:"))
            {
                return HandleAppLaunch(cmdLower);
            }
            
            if (cmdLower.StartsWith("screenshot"))
            {
                return await HandleScreenshotAsync(cmdLower);
            }

            return CommandResult.Error("ERROR: 未知指令", "未知指令");
        }

        private CommandResult HandleSleep()
        {
            SetSuspendState(false, true, false);
            return CommandResult.Ok("OK: 电脑即将进入睡眠", "执行: 电脑睡眠");
        }

        private CommandResult HandleShutdown()
        {
            System.Diagnostics.Process.Start("shutdown", "/s /t 0");
            return CommandResult.Ok("OK: 电脑即将关机", "执行: 电脑关机");
        }

        private CommandResult HandleStatus()
        {
            return CommandResult.Ok("Running", "响应: Running");
        }

        private CommandResult HandleAppLaunch(string command)
        {
            string appName = command.Substring(4).Trim();
            var (success, message) = _appLauncher.Launch(appName);
            
            if (success)
            {
                return CommandResult.Ok($"OK: {message}", message);
            }
            return CommandResult.Error($"ERROR: {message}", message);
        }

        private async Task<CommandResult> HandleScreenshotAsync(string command)
        {
            int screenIndex = 0;
            
            if (command.StartsWith("screenshot:"))
            {
                string param = command.Substring(11).Trim();
                if (int.TryParse(param, out int idx))
                {
                    screenIndex = idx - 1;
                }
            }

            var (success, data, message) = await _screenshotService.CaptureAsync(screenIndex);
            
            if (success)
            {
                return CommandResult.Ok($"SCREENSHOT:{data}", message);
            }
            return CommandResult.Error($"ERROR: {message}", message);
        }

        public void RegisterApp(string name, string path)
        {
            _appLauncher.RegisterApp(name, path);
        }
    }
}
