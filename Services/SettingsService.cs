using System;
using System.IO;
using System.Text.Json;
using SleepOnLan.Models;

namespace SleepOnLan.Services
{
    public class SettingsService
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SleepOnLan",
            "settings.json"
        );

        private AppSettings _settings;

        public AppSettings Settings => _settings;

        public SettingsService()
        {
            _settings = Load();
        }

        public AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch { }
            
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                string? directory = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }

        public void UpdatePort(int port)
        {
            _settings.DefaultPort = port;
            Save();
        }

        public void AddApp(string name, string path)
        {
            _settings.RegisteredApps.Add(new AppInfo { Name = name, Path = path });
            Save();
        }

        public void RemoveApp(string name)
        {
            _settings.RegisteredApps.RemoveAll(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            Save();
        }

        public void UpdateApp(string oldName, string newName, string newPath)
        {
            var app = _settings.RegisteredApps.Find(a => a.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase));
            if (app != null)
            {
                app.Name = newName;
                app.Path = newPath;
                Save();
            }
        }
    }
}
