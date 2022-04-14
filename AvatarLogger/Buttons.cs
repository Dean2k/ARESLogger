using MelonLoader;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.QuickMenu;
using ReModAres.Core.UI.Wings;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.UI;

namespace AvatarLogger
{
    internal static class Buttons
    {
        private static GameObject _socialMenuInstance;

        public static GameObject GetSocialMenuInstance()
        {
            if (_socialMenuInstance == null)
            {
                _socialMenuInstance = GameObject.Find("UserInterface/MenuContent/Screens");
            }
            return _socialMenuInstance;
        }

        public static Sprite ButtonImage = BaseFunctions.LoadSpriteFromDisk((Environment.CurrentDirectory + "\\ARESLogo.png"));
        private static ConfigHelper<AvatarLogger.Config> Helper => AvatarLogger.Main.Helper;
        
        public static void OnUiManagerInit()
        {
            MelonLogger.Msg("Ui initiating...");

            MelonLogger.Msg("ARES Favorites Go BRRR");

            ReMirroredWingMenu wingMenu = ReMirroredWingMenu.Create("ARES", "Open the ARES menu", ButtonImage);
            ReMirroredWingMenu lsmp = wingMenu.AddSubMenu("Log Settings", "Allows you to configure your ARES settings!");
            lsmp.AddToggle("Log Worlds", "Toggles the logging of worlds", delegate (bool b) { Main.Config.LogWorlds = b; }, Main.Config.LogWorlds);
            lsmp.AddToggle("Log Avatars", "Toggles the logging of avatars", delegate (bool b) { Main.Config.LogAvatars = b; }, Main.Config.LogAvatars);
            lsmp.AddToggle("Log Public Avatars", "Toggles the logging of public avatars", delegate (bool b) { Main.Config.LogPublicAvatars = b; }, Main.Config.LogPublicAvatars);
            lsmp.AddToggle("Log Private Avatars", "Toggles the logging of private avatars", delegate (bool b) { Main.Config.LogPrivateAvatars = b; }, Main.Config.LogPrivateAvatars);
            lsmp.AddToggle("Log Own Avatars", "Toggles the logging of own avatars", delegate (bool b) { Main.Config.LogOwnAvatars = b; }, Main.Config.LogOwnAvatars);
            lsmp.AddToggle("Log Friends Avatars", "Toggles the ability to log avatars uploaded to your friends accounts!", delegate (bool b) { Main.Config.LogFriendsAvatars = b; }, Main.Config.LogFriendsAvatars);
            lsmp.AddToggle("Log To Console", "Toggles the ability display logged avatars in console!", delegate (bool b) { Main.Config.LogToConsole = b; }, Main.Config.LogToConsole);
            lsmp.AddToggle("Log Errors To Console", "Toggles the ability display why avatars weren't logged in console!", delegate (bool b) { Main.Config.ConsoleError = b; }, Main.Config.ConsoleError);

            ReMirroredWingMenu fPage = wingMenu.AddSubMenu("Functions", "Use the other features within ARES");
            fPage.AddButton("Open ARES GUI", "Opens the ARES GUI on your desktop!", delegate { OpenGui(); });
            fPage.AddButton("Copy Instance ID", "Copies the current instance ID to your clipboard!", delegate { Clipboard.SetText(Main.WorldInstanceID); });
            fPage.AddButton("Join Instance By ID", "Joins the instance currently within your clipboard!", JoinInstanceById);
            fPage.AddButton("Wear Avatar ID", "Changes into avatar ID that is currently in clipboard!", ChangeAvatar);
            fPage.AddButton("Show Logging Statistics", "Displays session statistics within the console", ShowSessionStats);

            ReMirroredWingMenu otherToggles = wingMenu.AddSubMenu("Other", "Use the other features within ARES");
            otherToggles.AddToggle("Stealth Mode", "Hides all in-game indicators that you are running ARES (requires restart!)", delegate (bool b) { Main.Config.Stealth = b; }, Main.Config.Stealth);
            otherToggles.AddToggle("HWID Spoof", "Spoof your HWID incase you've been banned etc!", delegate (bool b) { Main.Config.HWIDSpoof = b; }, Main.Config.HWIDSpoof);
            otherToggles.AddToggle("Auto Update", "Allow the plugin to auto update!", delegate (bool b) { Main.Config.AutoUpdate = b; }, Main.Config.AutoUpdate);
            fPage.AddButton("Restart VRC", "Restarts VRChat!", delegate { RestartVrChat(false); });
            fPage.AddButton("Restart VRC (Persistent)", "Restarts VRChat and re-joins the room you were in!", delegate { RestartVrChat(true); });

            UiManager uiManager = new UiManager("ARES Logger", ButtonImage);

            ReMenuPage lsmpt = null;
            try
            {
                lsmpt = uiManager.MainMenu.AddMenuPage("Logging Settings", "Allows you to configure your ARES settings!");
            }
            catch { }

            lsmpt.AddToggle("Log Worlds", "Toggles the logging of worlds", delegate (bool b) { Main.Config.LogWorlds = b; }, Main.Config.LogWorlds);
            lsmpt.AddToggle("Log Avatars", "Toggles the logging of avatars", delegate (bool b) { Main.Config.LogAvatars = b; }, Main.Config.LogAvatars);
            lsmpt.AddToggle("Log Public Avatars", "Toggles the logging of public avatars", delegate (bool b) { Main.Config.LogPublicAvatars = b; }, Main.Config.LogPublicAvatars);
            lsmpt.AddToggle("Log Private Avatars", "Toggles the logging of private avatars", delegate (bool b) { Main.Config.LogPrivateAvatars = b; }, Main.Config.LogPrivateAvatars);
            lsmpt.AddToggle("Log Own Avatars", "Toggles the logging of own avatars", delegate (bool b) { Main.Config.LogOwnAvatars = b; }, Main.Config.LogOwnAvatars);
            lsmpt.AddToggle("Log Friends Avatars", "Toggles the ability to log avatars uploaded to your friends accounts!", delegate (bool b) { Main.Config.LogFriendsAvatars = b; }, Main.Config.LogFriendsAvatars);
            lsmpt.AddToggle("Log To Console", "Toggles the ability display logged avatars in console!", delegate (bool b) { Main.Config.LogToConsole = b; }, Main.Config.LogToConsole);
            lsmpt.AddToggle("Log Errors To Console", "Toggles the ability display why avatars weren't logged in console!", delegate (bool b) { Main.Config.ConsoleError = b; }, Main.Config.ConsoleError);

            ReMenuPage fPageT = null;
            try
            {
                fPageT = uiManager.MainMenu.AddMenuPage("ARES Functions", "Use the other features within ARES");
            }
            catch { }

            fPageT.AddButton("Open ARES GUI", "Opens the ARES GUI on your desktop!", OpenGui);
            fPageT.AddButton("Copy Instance ID", "Copies the current instance ID to your clipboard!", delegate { Clipboard.SetText(Main.WorldInstanceID); });
            fPageT.AddButton("Join Instance By ID", "Joins the instance currently within your clipboard!", JoinInstanceById);
            fPageT.AddButton("Wear Avatar ID", "Changes into avatar ID that is currently in clipboard!", ChangeAvatar);
            fPageT.AddButton("Restart VRC", "Restarts VRChat!", delegate { RestartVrChat(false); });
            fPageT.AddButton("Restart VRC (Persistent)", "Restarts VRChat and re-joins the room you were in!", delegate { RestartVrChat(true); });
            fPageT.AddButton("Show Logging Statistics", "Displays session statistics within the console", ShowSessionStats);
            fPageT.AddToggle("Custom Nameplates", "Shows Custom Nameplates (reload world to fully unload)", delegate (bool b) { Main.Config.CustomNameplates = b; CustomNamePlate(b); }, Main.Config.CustomNameplates);
            fPageT.AddToggle("Stealth Mode", "Hides all in-game indicators that you are running ARES (Requires restart!)", delegate (bool b) { Main.Config.Stealth = b; RestartVrChat(true); }, Main.Config.Stealth);
            fPageT.AddToggle("HWID Spoof", "Spoof your HWID incase you've been banned etc!", delegate (bool b) { Main.Config.HWIDSpoof = b; }, Main.Config.HWIDSpoof);
            fPageT.AddToggle("Auto Update", "Allow the plugin to auto update!", delegate (bool b) { Main.Config.AutoUpdate = b; }, Main.Config.AutoUpdate);
            MelonLogger.Msg("Ui ready!");
        }

        //Restarts VRChat
        public static void RestartVrChat(bool persistence)
        {
            new Thread(() =>
            {
                UnityEngine.Application.Quit();
                Thread.Sleep(2500);
                try
                {
                    string cl = Environment.CommandLine;
                    if (cl.Contains("vrchat://launch"))
                    {
                        string launch = cl.Substring(cl.IndexOf("vrchat://launch"));
                        cl = cl.Remove(cl.IndexOf("vrchat://launch"), launch.Contains(" ") ? launch.IndexOf(" ") : launch.Length);
                    }
                    if (persistence) { cl = $"{cl} vrchat://launch?id={RoomManager.field_Internal_Static_ApiWorld_0.id}:{RoomManager.field_Internal_Static_ApiWorldInstance_0.instanceId}"; }
                    Process.Start($"{Environment.CurrentDirectory}\\VRChat.exe", cl);
                }
                catch (Exception) { new Exception(); }
                Process.GetCurrentProcess().Kill();
            })
            {
                IsBackground = true,
                Name = "RestartVRC Thread"
            }.Start();
        }

        public static void CustomNamePlate(bool enable)
        {
            if (enable)
            {
                try
                {
                    foreach (Player player in UnityEngine.Object.FindObjectsOfType<Player>())
                    {

                        CustomNameplate nameplate = player.transform.Find("Player Nameplate/Canvas/Nameplate").gameObject.AddComponent<AvatarLogger.CustomNameplate>();
                        nameplate.Player = player;

                    }
                }
                catch { }
            }
            else
            {
                foreach (Player player in UnityEngine.Object.FindObjectsOfType<Player>())
                {
                    CustomNameplate disabler = player.transform.Find("Player Nameplate/Canvas/Nameplate").gameObject.GetComponent<AvatarLogger.CustomNameplate>();
                    disabler.Dispose();
                }
            }
        }
        
        private static void ShowSessionStats()
        {
            MelonLogger.Msg("-------------------------------------");
            MelonLogger.Msg($"Current Logging Session Stats:");
            MelonLogger.Msg("-------------------------------------");
            MelonLogger.Msg($"Total Logged Avatars: {Main.Pub + Main.Pri}");
            MelonLogger.Msg($"Logged PC Avatars: {Main.PC}");
            MelonLogger.Msg($"Logged Quest Avatars: {Main.Q}");
            MelonLogger.Msg($"Logged Private Avatars: {Main.Pri}");
            MelonLogger.Msg($"Logged Public Avatars: {Main.Pub}");
            MelonLogger.Msg("-------------------------------------");
        }
        
        private static void JoinInstanceById()
        {
            string[] ID = Clipboard.GetText().Split(':');
            if (Clipboard.GetText().Contains("wrld"))
            {
                Main.JoinInstance(ID[0], ID[1]);
                MelonLogger.Msg($"Instance joined: {Clipboard.GetText()}");
            }
            else
            {
                MelonLogger.Msg($"Invalid instance ID!");
            }
        }
        
        public static void ChangeAvatar()
        {
            Regex avatar = new Regex("avtr_[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}");
            if (avatar.IsMatch(Clipboard.GetText()))
            {
                new ApiAvatar { id = Clipboard.GetText() }.Get(new Action<ApiContainer>(x =>
                {
                    GetSocialMenuInstance().transform.Find("Avatar").GetComponent<PageAvatar>().field_Public_SimpleAvatarPedestal_0.field_Internal_ApiAvatar_0 = x.Model.Cast<ApiAvatar>();
                    GetSocialMenuInstance().transform.Find("Avatar").GetComponent<PageAvatar>().ChangeToSelectedAvatar();
                }), new Action<ApiContainer>(x =>
                {
                    MelonLogger.Msg($"Failed to change to avatar: {Clipboard.GetText()} | Error Message: {x.Error}");
                }));
            }
            else
            {
                MelonLogger.Msg($"Invalid Avatar ID!");
            }
        }
        
        public static void OpenGui()
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName("ARES"))
                {
                    proc.Kill();
                    MelonLogger.Msg("Pre-existent ARES closed!");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg("Error closing ARES :\n" + ex.Message);
            }
            try
            {
                foreach (Process proc in Process.GetProcessesByName("Unity"))
                {
                    proc.Kill();
                    MelonLogger.Msg("Pre-existent Unity closed!");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg("Error closing Unity :\n" + ex.Message);
            }
            try
            {
                foreach (Process proc in Process.GetProcessesByName("Unity Hub"))
                {
                    proc.Kill();
                    MelonLogger.Msg("Pre-existent Unity Hub closed!");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg("Error closing Unity Hub :\n" + ex.Message);
            }

            try
            {
                foreach (Process proc in Process.GetProcessesByName("AssetRipperConsole"))
                {
                    proc.Kill();
                    MelonLogger.Msg("Pre-existent AssetRipperConsole closed!");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg("Error closing AssetRipperConsole :\n" + ex.Message);
            }
            Directory.SetCurrentDirectory(MelonUtils.GameDirectory + "\\GUI\\");
            Process.Start("ARES.exe");
            Directory.SetCurrentDirectory(MelonUtils.GameDirectory);
            MelonLogger.Msg("ARES GUI Launched!");
        }
    }
}