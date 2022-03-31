using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using HarmonyLib;

#pragma warning disable IDE0044

namespace ComfyUtils
{
    public class ConfigHelper<T> where T : class
    {
        private HarmonyLib.Harmony HarmonyInstance = new HarmonyLib.Harmony($"ConfigHelper_[{typeof(T)}]");
        private static ConfigHelper<T> Instance;
        public event Action OnConfigUpdated;
        private string ConfigPath;
        private bool SaveOnUpdate;
        public T Config;
        public ConfigHelper(string configPath, bool saveOnUpdate = false)
        {
            Instance = this;
            ConfigPath = configPath;
            SaveOnUpdate = saveOnUpdate;

            if (!File.Exists(ConfigPath))
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Activator.CreateInstance(typeof(T)), Formatting.Indented));
            }
            Config = JsonConvert.DeserializeObject<T>(File.ReadAllText(ConfigPath));
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config, Formatting.Indented));

            FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(ConfigPath), Path.GetFileName(ConfigPath))
            {
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            watcher.Changed += FileUpdated;

            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                HarmonyInstance.Patch(property.GetSetMethod(),
                postfix: new HarmonyMethod(GetType().GetMethod(nameof(PropertyUpdated), BindingFlags.NonPublic | BindingFlags.Static)));
            }
        }
        private void FileUpdated(object obj, FileSystemEventArgs args)
        {
            T FileConfig = JsonConvert.DeserializeObject<T>(File.ReadAllText(ConfigPath));
            foreach (PropertyInfo property in FileConfig.GetType().GetProperties())
            {
                PropertyInfo property0 = Config.GetType().GetProperty(nameof(property.Name));
                if (property0 == null)
                {
                    continue;
                }
                if (property.GetValue(FileConfig) != property0.GetValue(Config))
                {
                    Config = FileConfig;
                    OnConfigUpdated?.Invoke();
                    break;
                }
            }
        }
        private static void PropertyUpdated()
        {
            if (Instance.SaveOnUpdate)
            {
                Instance.SaveConfig();
            }
            Instance.OnConfigUpdated?.Invoke();
        }
        public void SaveConfig() => File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config, Formatting.Indented));
    }
}
