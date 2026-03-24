using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace SleepOnLan.Services
{
    public class AutoStartManager
    {
        private const string RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "SleepOnLan";

        public bool IsEnabled
        {
            get
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, false);
                return key?.GetValue(AppName) != null;
            }
        }

        public (bool Success, string Message) Enable()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
                string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath))
                {
                    return (false, "无法获取程序路径");
                }
                
                key?.SetValue(AppName, $"\"{exePath}\" /minimized");
                return (true, "已启用开机自启");
            }
            catch (Exception ex)
            {
                return (false, $"启用开机自启失败: {ex.Message}");
            }
        }

        public (bool Success, string Message) Disable()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
                key?.DeleteValue(AppName, false);
                return (true, "已禁用开机自启");
            }
            catch (Exception ex)
            {
                return (false, $"禁用开机自启失败: {ex.Message}");
            }
        }
    }
}
