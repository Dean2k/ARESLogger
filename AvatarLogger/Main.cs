using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using VRC;
using VRC.UI.Core;
using static AvatarLogger.Logging;
using static AvatarLogger.Patches;
using static AvatarLogger.Buttons;
using Main = AvatarLogger.Main;
using Object = UnityEngine.Object;

//using System.Data.SQLite;
//Melon mod information
[assembly: MelonGame("VRChat")]
[assembly: MelonInfo(typeof(Main), "A.R.E.S Logger", "4.2.7", "By ShrekamusChrist, LargestBoi")]
[assembly: MelonColor(ConsoleColor.Yellow)]

namespace AvatarLogger
{
    public class Main : MelonMod
    {
        public static int PC = 0;
        public static int Q = 0;
        public static int Pub = 0;
        public static int Pri = 0;
        public static ConfigHelper<Config> Helper;
        private readonly Dictionary<string, string> _upkeepFiles = new Dictionary<string, string>();
        public static string WorldInstanceID =>
            $"{RoomManager.field_Internal_Static_ApiWorld_0.id}:{RoomManager.field_Internal_Static_ApiWorldInstance_0.instanceId}";
        public static Config Config => Helper.Config;
        
        public static void JoinInstance(string worldId, string instanceId)
        {
            new PortalInternal().Method_Private_Void_String_String_PDM_0(worldId, instanceId);
        }
        
        public override void OnApplicationStart()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            foreach (string item in arguments)
            {
                if (item.ToLower() == "DaddyUwU")
                {
                    MelonLogger.Msg("Skipping loading of Avatar logger (Application bot)");
                    return;
                }
            }

            Helper = new ConfigHelper<Config>($"{MelonUtils.UserDataDirectory}\\ARESConfig.json", true);
            
            if (Config.AutoUpdate)
                _upkeepFiles.Add($"{MelonHandler.PluginsDirectory}\\ARESPlugin.dll",
                    "https://github.com/Dean2k/A.R.E.S/releases/latest/download/ARESPlugin.dll");
            BaseFunctions.HandleQueue(_upkeepFiles);
            try
            {
                MelonLogger.Msg("Applying patches...");
                if (Config.HWIDSpoof)
                {
                    Patches.HWIDPatch();
                    MelonLogger.Msg("HWID patched");
                }

                Patches.AllowAvatarCopyingPatch();
                MelonLogger.Msg("Avatar cloning patched, force clone enabled!");
                Patches.OnEventPatch();
                try
                {
                    ClassInjector.RegisterTypeInIl2Cpp<CustomNameplate>();
                }
                catch
                {
                    MelonLogger.Msg("Failed to inject");
                }

                MelonLogger.Msg("OnEvent patch applied (1/2)");
                MelonCoroutines.Start(OnNetworkManagerInit());
                MelonCoroutines.Start(WaitForUiManager());
                MelonLogger.Msg("Network manager patched (2/2)");
                MelonLogger.Msg("Avatars can now be logged!");
            }
            catch (Exception ex)
            {
                MelonLogger.Msg("Failed patches!" + ex.Message);
            }
            
            if (!Config.Stealth)
            {
                MelonCoroutines.Start(FindUi());
                MelonLogger.Msg("Listening for Ui...");
            }
            else
            {
                MelonLogger.Msg(
                    "ARES running in stealth mode! To restore your in-game buttons enable 'Stealth' in the settings category in your GUI!");
            }
        }

        private IEnumerator WaitForUiManager()
        {
            while (VRCUiManager.field_Private_Static_VRCUiManager_0 == null) yield return null;

            var playerJoinedDelegate = NetworkManager.field_Internal_Static_NetworkManager_0
                .field_Internal_VRCEventDelegate_1_Player_0;

            playerJoinedDelegate.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>(p =>
            {
                if (p != null) OnPlayerJoined(p);
            }));
        }

        public static void OnPlayerJoined(Player player)
        {
            if (Config.CustomNameplates)
            {
                var nameplate = player.transform.Find("Player Nameplate/Canvas/Nameplate").gameObject
                    .AddComponent<CustomNameplate>();
                nameplate.Player = player;
            }
        }

        
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (buildIndex == -1)
            {
                MelonCoroutines.Start(FetchFriends());
                MelonCoroutines.Start(LogWorlds());
            }
        }
        
        internal static IEnumerator OnNetworkManagerInit()
        {
            while (NetworkManager.field_Internal_Static_NetworkManager_0 == null)
                yield return new WaitForSecondsRealtime(2f);

            if (NetworkManager.field_Internal_Static_NetworkManager_0 != null)
                new Action(() =>
                {
                    NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0
                        .field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>(obj =>
                        {
                            ExecuteLog(obj);
                        }));
                })();
        }
        
        internal static IEnumerator LogWorlds()
        {
            while (RoomManager.field_Internal_Static_ApiWorld_0 == null) yield return new WaitForEndOfFrame();
            var apiWorld = RoomManager.field_Internal_Static_ApiWorld_0;
            ExecuteLogWorld(apiWorld);
        }
        
        private static IEnumerator FindUi()
        {
            while (UIManager.prop_UIManager_0 == null) yield return null;
            while (Object.FindObjectOfType<VRC.UI.Elements.QuickMenu>() == null) yield return null;
            while (GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)") == null) yield return null;
            OnUiManagerInit();
        }
    }
}