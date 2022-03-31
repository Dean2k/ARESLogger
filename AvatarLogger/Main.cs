//Inporting all refernces used within the mod
using System;
using System.IO;
using System.Collections.Generic;
using MelonLoader;
using VRC.Core;
using VRC.UI.Core;
using Newtonsoft.Json;
using UnityEngine;
using ComfyUtils;
using static Logging.Logging;
using static BaseFuncs.BaseFuncs;
using static Patches.Patches;
using static Buttons.Buttons;
using UnhollowerRuntimeLib;
using VRC;
using System.Collections;
//using System.Data.SQLite;
//Melon mod information
[assembly: MelonGame("VRChat")]
[assembly: MelonInfo(typeof(AvatarLogger.Main), "A.R.E.S Logger", "4.2.4", "By ShrekamusChrist, LargestBoi, Yui and Afton")]
[assembly: MelonColor(ConsoleColor.Yellow)]

namespace AvatarLogger
{
    public class Main : MelonMod
    {
        //Makes a dictionary to store all files that mey need to be updated to ensure consistent stability of ARES
        private Dictionary<string, string> UpkeepFiles = new Dictionary<string, string>();
        //Creates string that can retrieve the current instance ID
        public static string WorldInstanceID => $"{RoomManager.field_Internal_Static_ApiWorld_0.id}:{RoomManager.field_Internal_Static_ApiWorldInstance_0.instanceId}";
        //Function that allows the world to be joined via an instanc ID
        public static void JoinInstance(string worldID, string instanceID) => new PortalInternal().Method_Private_Void_String_String_PDM_0(worldID, instanceID);

        //Sets static counter values to monitor logging statistics
        public static int PC = 0;
        public static int Q = 0;
        public static int Pub = 0;
        public static int Pri = 0;
        public static ConfigHelper<Config> Helper;
        public static Config Config => Helper.Config;
        //public static SQLiteConnection sqlite_conn;
        //Void to run on application start
        public override void OnApplicationStart()
        {
            //Create file for ARES favorites
            if (!File.Exists("UserData/ARES_Favorites_config.json"))
                File.Create("UserData/ARES_Favorites_config.json");
            Helper = new ConfigHelper<Config>($"{MelonUtils.UserDataDirectory}\\ARESConfig.json", true);
           
            //Ensures reqired upkeep files are installed and updated
            if (Config.AutoUpdate)
            {
                UpkeepFiles.Add($"{MelonHandler.PluginsDirectory}\\ARESPlugin.dll", "https://github.com/Dean2k/A.R.E.S/releases/latest/download/ARESPlugin.dll");
                HandleQueue(UpkeepFiles);
            }
            try
            {
                MelonLogger.Msg("Applying patches...");
                if (Config.HWIDSpoof)
                {
                    HWIDPatch();
                    MelonLogger.Msg("HWID patched");
                }
                AllowAvatarCopyingPatch();
                MelonLogger.Msg("Avatar cloning patched, force clone enabled!");
                OnEventPatch();
                try
                {
                    ClassInjector.RegisterTypeInIl2Cpp<CustomNameplate>();
                } catch
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
            //Starts lister to await Ui
            if (!Config.Stealth)
            {
                MelonCoroutines.Start(FindUI());
                MelonLogger.Msg("Listening for Ui...");
            }
            else { MelonLogger.Msg("ARES running in stealth mode! To restore your in-game buttons enable 'Stealth' in the settings category in your GUI!"); }


           
        }

        private IEnumerator WaitForUiManager()
        {
            while (VRCUiManager.field_Private_Static_VRCUiManager_0 == null) yield return null;

            var playerJoinedDelegate = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0;

                playerJoinedDelegate.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>(p =>
                {
                    if (p != null) OnPlayerJoined(p);
                }));
            
        }

        public static void OnPlayerJoined(Player player)
        {
            if (Config.CustomNameplates)
            {
                CustomNameplate nameplate = player.transform.Find("Player Nameplate/Canvas/Nameplate").gameObject.AddComponent<CustomNameplate>();
                nameplate.player = player;
            }
        }


        //Wait till scene loads to being getting updated friends list
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            //When scene loads fetch friends
            if (buildIndex == -1)
            {
                MelonCoroutines.Start(FetchFriends());
                MelonCoroutines.Start(LogWorlds());
            }
        }
        //On network manager run a command thatll log all users in a room
        internal static System.Collections.IEnumerator OnNetworkManagerInit()
        {
            //Obtains the player hash table and sends it to the method responsible for logging information from the table
            while (NetworkManager.field_Internal_Static_NetworkManager_0 == null) yield return new UnityEngine.WaitForSecondsRealtime(2f);

            if (NetworkManager.field_Internal_Static_NetworkManager_0 != null) new Action(() =>
            {
                NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<VRC.Player>((obj) =>
                {                   
                    var ht = obj.field_Private_Player_0.field_Private_Hashtable_0;
                    dynamic playerHashtable = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(Serialize.FromIL2CPPToManaged<object>(ht)));
                    ExecuteLog(playerHashtable);
                }));
            })();
        }

        //On network manager run a command thatll log worlds
        internal static System.Collections.IEnumerator LogWorlds()
        {
            while (RoomManager.field_Internal_Static_ApiWorld_0 == null) yield return new WaitForEndOfFrame();
            ApiWorld apiWorld = RoomManager.field_Internal_Static_ApiWorld_0;
            ExecuteLogWorld(apiWorld);
        }

        //Locates the Ui in preperation for button creation
        private static System.Collections.IEnumerator FindUI()
        {
            while (UIManager.prop_UIManager_0 == null) yield return null;
            while (UnityEngine.Object.FindObjectOfType<VRC.UI.Elements.QuickMenu>() == null) yield return null;
            while (GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)") == null) yield return null;
            //Once the Ui has launched get variables and begin Ui creation
            OnUiManagerInit();
        }
    }
}
