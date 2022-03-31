//Importing reqired modules
using ExitGames.Client.Photon;
using Harmony;
using MelonLoader;
using Newtonsoft.Json;
using Photon.Realtime;
using System;
using System.Reflection;
using UnityEngine;
using VRC.Core;
using static BaseFuncs.BaseFuncs;
using static Logging.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

//Contains all patches ARES makes to VRChat
namespace Patches
{
    internal static class Patches
    {
        //Creates new instance to patch on
        private static HarmonyLib.Harmony Instance = new HarmonyLib.Harmony("ARES");
        private static string newHWID = "";

        //Enables avatar cloning regadless of what the person has their clone setting on
        public static void AllowAvatarCopyingPatch()
        {
            Instance.Patch(typeof(APIUser).GetProperty(nameof(APIUser.allowAvatarCopying)).GetSetMethod(), new HarmonyLib.HarmonyMethod(typeof(Patches).GetMethod(nameof(Patches.ForceClone), BindingFlags.NonPublic | BindingFlags.Static)));
        }

        public static void HWIDPatch()
        {
            Instance.Patch(typeof(SystemInfo).GetProperty("deviceUniqueIdentifier").GetGetMethod(), new HarmonyLib.HarmonyMethod(HarmonyLib.AccessTools.Method(typeof(Patches), nameof(Patches.FakeHWID))));
        }

        private static bool FakeHWID(ref string __result)
        {
            if (Patches.newHWID == "")
            {
                Patches.newHWID = KeyedHashAlgorithm.Create().ComputeHash(Encoding.UTF8.GetBytes(string.Format("{0}A-{1}{2}-{3}{4}-{5}{6}-3C-1F", new object[]
                {
                    new System.Random().Next(0, 9),
                    new System.Random().Next(0, 9),
                    new System.Random().Next(0, 9),
                    new System.Random().Next(0, 9),
                    new System.Random().Next(0, 9),
                    new System.Random().Next(0, 9),
                    new System.Random().Next(0, 9)
                }))).Select(delegate (byte x)
                {
                    byte b = x;
                    return b.ToString("x2");
                }).Aggregate((string x, string y) => x + y);
                MelonLogger.Msg("[HWID] new " + Patches.newHWID);
            }
            __result = Patches.newHWID;
            return false;
        }

        private static void ForceClone(ref bool __0) => __0 = true;

        //All the possible routes leading to an avatar being logged
        public static void OnEventPatch()
        {
            Instance.Patch(typeof(VRCNetworkingClient).GetMethod("OnEvent"), new HarmonyLib.HarmonyMethod(typeof(Patches).GetMethod(nameof(Patches.OnEventLBC), BindingFlags.NonPublic | BindingFlags.Static)), null, null, null, null);
        }

        private static bool OnEventLBC(EventData __0)
        {
            var eventCode = __0.Code;
            switch (eventCode)
            {
                case 1:
                case 3:
                    return true;

                case 4:
                    return true;

                case 6:
                    return true;

                case 7:
                    return true;

                case 9:
                    return true;

                case 33:
                    return true;

                case 209:
                    return true;

                case 210:
                    return true;

                case 253:
                    {
                        // patched by LargestBoi
                        try
                        {
                            foreach (VRCPlayer player in UnityEngine.Object.FindObjectsOfType<VRCPlayer>())
                            {
                                var ht = player.prop_Player_0.prop_Player_1.prop_Hashtable_0;
                                dynamic playerHashtable = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(Serialize.FromIL2CPPToManaged<object>(ht)));
                                ExecuteLog(playerHashtable, true);
                            }
                        }
                        catch (Exception e) { MelonLogger.Msg($"Error: \n{e}"); }
                    }
                    return true;
                default:
                    break;
            }
            return true;
        }
    }
}