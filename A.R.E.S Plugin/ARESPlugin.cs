using MelonLoader;
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Security.Cryptography;

[assembly: MelonInfo(typeof(ARESPlugin.Updater), "ARES Manager", "1.5", "ShrekamusChrist, LargestBoi (Retired)")]
[assembly: MelonColor(ConsoleColor.Yellow)]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ARESPlugin
{
    public class Updater : MelonPlugin
    {
        private Dictionary<string, string> _files = new Dictionary<string, string>();

        public override void OnPreInitialization()
        {
            bool skipUpdates = false;
            string[] arguments = Environment.GetCommandLineArgs();
            foreach (string item in arguments)
            {
                if(item.ToLower() == "-shrekno")
                {
                    skipUpdates = true;
                    MelonLogger.Msg("Skipping Updating ARES");
                }
            }
            
            if (!skipUpdates)
            {
                _files.Add($"{MelonHandler.ModsDirectory}\\AvatarLogger.dll", "https://github.com/Dean2k/A.R.E.S/releases/latest/download/AvatarLogger.dll");
                _files.Add($"{MelonUtils.GameDirectory}\\ReModAres.Core.dll", "https://github.com/Dean2k/ReModCE/releases/latest/download/ReModAres.Core.dll");
                _files.Add($"{MelonUtils.GameDirectory}\\ARESLogo.png", "https://github.com/Dean2k/A.R.E.S/releases/latest/download/ARESLogo.png");

                foreach (KeyValuePair<string, string> pair in _files)
                {
                    string name = pair.Key.Substring(pair.Key.LastIndexOf('\\') + 1);
                    if (File.Exists(pair.Key))
                    {
                        var oldHash = Sha256CheckSum(pair.Key);
                        DownloadMod(pair);
                        if (Sha256CheckSum(pair.Key) != oldHash)
                        {
                            MelonLogger.Msg($"Updated: {name}!");
                        }
                    }
                    else
                    {
                        DownloadMod(pair);
                    }
                }
            }
        }

        private static void DownloadMod(KeyValuePair<string, string> pair)
        {
            using WebClient client = new WebClient();
            client.DownloadFile(pair.Value, pair.Key);
        }

        private string Sha256CheckSum(string filePath)
        {
            using var hash = SHA256.Create();
            using FileStream fileStream = File.OpenRead(filePath);
            return Convert.ToBase64String(hash.ComputeHash(fileStream));
        }
    }
}