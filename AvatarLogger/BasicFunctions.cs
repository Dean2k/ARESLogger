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
using static Buttons.Buttons;
using static AvatarLogger.Main;
//Contains all basic functions reqires for ARES to operate
namespace BaseFuncs
{
    internal static class BaseFuncs
    {

        internal class Serialize
        {
            public static byte[] GetByteArray(int sizeInKb)
            {
                System.Random random = new System.Random();
                byte[] array = new byte[sizeInKb * 1024];
                random.NextBytes(array);
                return array;
            }
            public static UnityEngine.Object ByteArrayToObjectUnity2(byte[] arrBytes)
            {
                Il2CppStructArray<byte> il2CppStructArray = new Il2CppStructArray<byte>((long)arrBytes.Length);
                arrBytes.CopyTo(il2CppStructArray, 0);
                Il2CppSystem.Object @object = new Il2CppSystem.Object(il2CppStructArray.Pointer);
                return new UnityEngine.Object(@object.Pointer);
            }
            public static byte[] ToByteArray(Il2CppSystem.Object obj)
            {
                if (obj == null) return null;
                var bf = new Il2CppSystem.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var ms = new Il2CppSystem.IO.MemoryStream();
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
            public static byte[] ToByteArray(object obj)
            {
                if (obj == null) return null;
                var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var ms = new System.IO.MemoryStream();
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
            public static T FromByteArray<T>(byte[] data)
            {
                if (data == null) return default(T);
                var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                using (var ms = new System.IO.MemoryStream(data))
                {
                    object obj = bf.Deserialize(ms);
                    return (T)obj;
                }
            }
            public static T IL2CPPFromByteArray<T>(byte[] data)
            {
                if (data == null) return default(T);
                var bf = new Il2CppSystem.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var ms = new Il2CppSystem.IO.MemoryStream(data);
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
            public static T FromIL2CPPToManaged<T>(Il2CppSystem.Object obj)
            {
                return FromByteArray<T>(ToByteArray(obj));
            }
            public static T FromManagedToIL2CPP<T>(object obj)
            {
                return IL2CPPFromByteArray<T>(ToByteArray(obj));
            }
        }
        //Funtion to load a sprite from an image on the disk provided a string/path
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
        //Function to get the SHA of a particular file
        public static string SHA256CheckSum(string filePath)
        {
            using (var hash = SHA256.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    return Convert.ToBase64String(hash.ComputeHash(fileStream));
                }
            }
        }
        //Function to handle the queue created
        public static void HandleQueue(Dictionary<string, string>  Queue)
        {
            foreach (KeyValuePair<string, string> pair in Queue)
            {
                string name = pair.Key.Substring(pair.Key.LastIndexOf('\\') + 1);
                if (File.Exists(pair.Key))
                {
                    var OldHash = SHA256CheckSum(pair.Key);
                    DownloadPlugin(pair);
                    if (SHA256CheckSum(pair.Key) != OldHash)
                    {
                        MelonLogger.Msg($"Updated: {name}! Restarting VRC...");
                        RVRC(false);
                    }
                }
                else
                {
                    MelonLogger.Msg($"{name} Not Found! Loading...");
                    DownloadPlugin(pair);
                    MelonLogger.Msg($"{name} Installed! Restarting VRC...");
                    RVRC(false);
                }
            }
        }
        //Downloads the files from the queue
        public static void DownloadPlugin(KeyValuePair<string, string> pair)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(pair.Value, pair.Key);
            }
        }
    }
}
