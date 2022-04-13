//Importing reqired modules

using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ExitGames.Client.Photon;
using HarmonyLib;
using MelonLoader;
using Photon.Realtime;
using UnityEngine;
using VRC.Core;
using Object = UnityEngine.Object;
using Random = System.Random;

//Contains all patches ARES makes to VRChat
namespace AvatarLogger
{
    internal static class Patches
    {
        //Creates new instance to patch on
        private static HarmonyLib.Harmony _instance = new HarmonyLib.Harmony("ARES");
        private static string newHWID = "";

        //Enables avatar cloning regadless of what the person has their clone setting on
        public static void AllowAvatarCopyingPatch()
        {
            _instance.Patch(typeof(APIUser).GetProperty(nameof(APIUser.allowAvatarCopying)).GetSetMethod(),
                new HarmonyMethod(typeof(Patches).GetMethod(nameof(ForceClone),
                    BindingFlags.NonPublic | BindingFlags.Static)));
        }

        public static void HWIDPatch()
        {
            _instance.Patch(typeof(SystemInfo).GetProperty("deviceUniqueIdentifier").GetGetMethod(),
                new HarmonyMethod(AccessTools.Method(typeof(Patches), nameof(FakeHWID))));
        }

        private static bool FakeHWID(ref string __result)
        {
            if (newHWID == "")
            {
                newHWID = KeyedHashAlgorithm.Create().ComputeHash(Encoding.UTF8.GetBytes(string.Format(
                    "{0}A-{1}{2}-{3}{4}-{5}{6}-3C-1F", new object[]
                    {
                        new Random().Next(0, 9),
                        new Random().Next(0, 9),
                        new Random().Next(0, 9),
                        new Random().Next(0, 9),
                        new Random().Next(0, 9),
                        new Random().Next(0, 9),
                        new Random().Next(0, 9)
                    }))).Select(delegate(byte x)
                {
                    byte b = x;
                    return b.ToString("x2");
                }).Aggregate((string x, string y) => x + y);
                MelonLogger.Msg("[HWID] new " + newHWID);
            }

            __result = newHWID;
            return false;
        }

        private static void ForceClone(ref bool __0) => __0 = true;

        //All the possible routes leading to an avatar being logged
        public static void OnEventPatch()
        {
            _instance.Patch(typeof(LoadBalancingClient).GetMethod("OnEvent"),
                new HarmonyMethod(typeof(Patches).GetMethod(nameof(OnEventLBC),
                    BindingFlags.NonPublic | BindingFlags.Static)), null, null, null, null);
        }

        private static bool OnEventLBC(EventData __0)
        {
            var eventCode = __0.Code;
            switch (eventCode)
            {
                case 42:
                    return LogAvatar();
                case 223:
                    return LogAvatar();
                default:
                    return true;
            }
        }


        private static bool LogAvatar()
        {
            try
            {
                foreach (VRCPlayer player in Object.FindObjectsOfType<VRCPlayer>())
                {
                    Logging.ExecuteLog(player._player, true);
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg($"Error: \n{e}");
            }

            return true;
        }
    }
}