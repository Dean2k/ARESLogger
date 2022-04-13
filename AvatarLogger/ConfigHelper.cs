using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;

#pragma warning disable IDE0044

namespace AvatarLogger
{
    public class ConfigHelper<T> where T : class
    {
        private static ConfigHelper<T> _instance;
        private readonly string _configPath;
        private readonly HarmonyLib.Harmony _harmonyInstance = new HarmonyLib.Harmony($"ConfigHelper_[{typeof(T)}]");
        private readonly bool _saveOnUpdate;
        public T Config;

        public ConfigHelper(string configPath, bool saveOnUpdate = false)
        {
            _instance = this;
            _configPath = configPath;
            _saveOnUpdate = saveOnUpdate;

            if (!File.Exists(_configPath))
                File.WriteAllText(_configPath,
                    JsonConvert.SerializeObject(Activator.CreateInstance(typeof(T)), Formatting.Indented));
            Config = JsonConvert.DeserializeObject<T>(File.ReadAllText(_configPath));
            File.WriteAllText(_configPath, JsonConvert.SerializeObject(Config, Formatting.Indented));

            var watcher = new FileSystemWatcher(Path.GetDirectoryName(_configPath), Path.GetFileName(_configPath))
            {
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            watcher.Changed += FileUpdated;

            foreach (var property in typeof(T).GetProperties())
                _harmonyInstance.Patch(property.GetSetMethod(),
                    postfix: new HarmonyMethod(GetType().GetMethod(nameof(PropertyUpdated),
                        BindingFlags.NonPublic | BindingFlags.Static)));
        }

        public event Action OnConfigUpdated;

        private void FileUpdated(object obj, FileSystemEventArgs args)
        {
            var fileConfig = JsonConvert.DeserializeObject<T>(File.ReadAllText(_configPath));
            foreach (var property in fileConfig.GetType().GetProperties())
            {
                var property0 = Config.GetType().GetProperty(nameof(property.Name));
                if (property0 == null) continue;
                if (property.GetValue(fileConfig) != property0.GetValue(Config))
                {
                    Config = fileConfig;
                    OnConfigUpdated?.Invoke();
                    break;
                }
            }
        }

        private static void PropertyUpdated()
        {
            if (_instance._saveOnUpdate) _instance.SaveConfig();
            _instance.OnConfigUpdated?.Invoke();
        }

        public void SaveConfig()
        {
            File.WriteAllText(_configPath, JsonConvert.SerializeObject(Config, Formatting.Indented));
        }
    }
}