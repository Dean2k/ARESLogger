//Importing all reqired modules
using System.IO;
using UnityEngine;
using MelonLoader;
using UnhollowerBaseLib;
using System;
using System.Threading;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Net;
using static AvatarLogger.Buttons;

namespace AvatarLogger
{
    public static class BaseFunctions
    {
        internal static Sprite LoadSpriteFromDisk(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            byte[] data = File.ReadAllBytes(path);

            if (data == null || data.Length <= 0)
            {
                return null;
            }
            Texture2D tex = new Texture2D(512, 512);
            if (!Il2CppImageConversionManager.LoadImage(tex, data))
            {
                return null;
            }
            Sprite sprite = Sprite.CreateSprite(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f, 0, 0, new Vector4(), false);
            sprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            return sprite;
        }

        public static string Sha256CheckSum(string filePath)
        {
            using (var hash = SHA256.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    return Convert.ToBase64String(hash.ComputeHash(fileStream));
                }
            }
        }

        public static void HandleQueue(Dictionary<string, string>  queue)
        {
            foreach (KeyValuePair<string, string> pair in queue)
            {
                string name = pair.Key.Substring(pair.Key.LastIndexOf('\\') + 1);
                if (File.Exists(pair.Key))
                {
                    var oldHash = Sha256CheckSum(pair.Key);
                    DownloadPlugin(pair);
                    if (Sha256CheckSum(pair.Key) != oldHash)
                    {
                        MelonLogger.Msg($"Updated: {name}! Restarting VRC...");
                        RestartVrChat(false);
                    }
                }
                else
                {
                    MelonLogger.Msg($"{name} Not Found! Loading...");
                    DownloadPlugin(pair);
                    MelonLogger.Msg($"{name} Installed! Restarting VRC...");
                    RestartVrChat(false);
                }
            }
        }
        public static void DownloadPlugin(KeyValuePair<string, string> pair)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(pair.Value, pair.Key);
            }
        }
    }
}
