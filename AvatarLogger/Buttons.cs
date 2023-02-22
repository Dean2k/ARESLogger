using ApolloCore.API.QM;
using MelonLoader;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
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
        // note: will probably be very laggy as we are getting EVERY gameobject, but it's only ran once.
        // note 2: should probably make game_objects a span to make it not allocate on heap and only on stack as it is
        // fairly large
        if (_socialMenuInstance == null)
        {
            GameObject user_interface = null;
            var game_objects = UnityEngine.Object.FindObjectsOfType(Il2CppType.Of<GameObject>());

            for (var i = 0; i < game_objects.Count; i++)
            {
                if (game_objects[i].name.Contains("UserInterface"))
                {
                    user_interface = game_objects[i].TryCast<GameObject>();
                }
            }

            if (!user_interface)
                throw new Exception("unable to find userinterface.");

            _socialMenuInstance = user_interface.transform.Find("MenuContent/Screens").gameObject;
        }
        return _socialMenuInstance;
    }

    public static Sprite ButtonImage =
        BaseFunctions.LoadSpriteFromDisk((Environment.CurrentDirectory + "\\ARESLogo.png"));
    private static ConfigHelper<AvatarLogger.Config> Helper => AvatarLogger.Main.Helper;

    public static void OnUiManagerInit()
    {
        MelonLogger.Msg("Ui initiating...");

        var tabMenu = new QMTabMenu("Settings for the ARES Logger", "ARES Logger", ButtonImage);
        var menu = new QMNestedButton(tabMenu, 1, 0, "Toggles", "ARES Menu Toggles", "ARES");
        var menu2 = new QMNestedButton(tabMenu, 2, 0, "Functions", "ARES Menu Functions", "ARES");

            var worldButton = new QMToggleButton(menu, 1, 0, "Log Worlds", delegate
            {
                Main.Config.LogWorlds = true;
            }
            , delegate
            {
                Main.Config.LogWorlds = false;
            }, "Toggles the logging of worlds", Main.Config.LogWorlds);

            var avatarButton = new QMToggleButton(menu, 2, 0, "Log Avatars", delegate
            {
                Main.Config.LogAvatars = true;
            }
            , delegate
            {
                Main.Config.LogAvatars = false;
            }, "Toggles the logging of avatars", Main.Config.LogAvatars);

            var avatarPublicButton = new QMToggleButton(menu, 3, 0, "Log Public Avatars", delegate
            {
                Main.Config.LogPublicAvatars = true;
            }
            , delegate
            {
                Main.Config.LogPublicAvatars = false;
            }, "Toggles the logging of public avatars", Main.Config.LogPublicAvatars    );

            var avatarPrivateButton = new QMToggleButton(menu, 4, 0, "Log Private Avatars", delegate
            {
                Main.Config.LogPrivateAvatars = true;
            }
            , delegate
            {
                Main.Config.LogPrivateAvatars = false;
            }, "Toggles the logging of private avatars", Main.Config.LogPrivateAvatars  );

            var avatarOwnButton = new QMToggleButton(menu, 1, 1, "Log Own Avatars", delegate
            {
                Main.Config.LogOwnAvatars = true;
            }
            , delegate
            {
                Main.Config.LogOwnAvatars = false;
            }, "Toggles the logging of own avatars", Main.Config.LogOwnAvatars);

            var avatarFriendsButton = new QMToggleButton(menu, 2, 1, "Log Friends Avatars", delegate
            {
                Main.Config.LogFriendsAvatars = true;
            }
            , delegate
            {
                Main.Config.LogFriendsAvatars = false;
            }, "Toggles the logging of friends avatars", Main.Config.LogFriendsAvatars);

            var logConsoleButton = new QMToggleButton(menu, 3, 1, "Log Avatars to Console", delegate
            {
                Main.Config.LogToConsole = true;
            }
            , delegate
            {
                Main.Config.LogToConsole = false;
            }, "Toggles the logging to console", Main.Config.LogToConsole);

            var logConsoleErrorButton = new QMToggleButton(menu, 4, 1, "Log Errors to console", delegate
            {
                Main.Config.ConsoleError = true;
            }
            , delegate
            {
                Main.Config.ConsoleError = false;
            }, "Toggles the logging to console", Main.Config.ConsoleError);

            var worldIdButton = new QMSingleButton(menu2, 1, 0, "Copy Instance ID", delegate
            {
                Clipboard.SetText(Main.WorldInstanceID);
            }, "Copies the current instance ID to your clipboard!");

            var avatarChangeButton = new QMSingleButton(menu2, 2, 0, "Wear Avatar ID", delegate
            {
                ChangeAvatar();
            }, "Changes into avatar ID that is currently in clipboard!");

            var restartChangeButton = new QMSingleButton(menu2, 3, 0, "Show Logging Statistics", delegate
            {
                ShowSessionStats();
            }, "Displays session statistics within the console");

            var namePlatesButton = new QMToggleButton(menu2, 1, 1, "Custom Nameplates", delegate
            {
                Main.Config.CustomNameplates = true;
                CustomNamePlate(true);
            }
            , delegate
            {
                Main.Config.CustomNameplates = false;
                CustomNamePlate(false);
            }, "Shows Custom Nameplates (reload world to fully unload)", Main.Config.CustomNameplates);

            var hwidSpoofButton = new QMToggleButton(menu2, 2, 1, "HWID Spoof", delegate
            {
                Main.Config.HWIDSpoof = true;
            }
            , delegate
            {
                Main.Config.HWIDSpoof = false;
            }, "Spoof your HWID incase you've been banned etc!", Main.Config.HWIDSpoof);

            MelonLogger.Msg("Ui ready!");
    }

    public static void CustomNamePlate(bool enable)
    {
            if (enable)
            {
                try
                {
                    foreach (Player player in UnityEngine.Object.FindObjectsOfType<Player>())
                    {

                        CustomNameplate nameplate = player._vrcplayer.field_Public_PlayerNameplate_0.gameObject
                                                        .AddComponent<AvatarLogger.CustomNameplate>();
                        nameplate.Player = player;
                    }
                }
                catch
                {
                }
            }
            else
            {
                foreach (Player player in UnityEngine.Object.FindObjectsOfType<Player>())
                {
                    CustomNameplate disabler =
                        VRCApplication.prop_VRCApplication_0.gameObject.GetComponent<AvatarLogger.CustomNameplate>();
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

    public static void ChangeAvatar()
    {
            Regex Avatar =
                new Regex("avtr_[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}");
            if (Avatar.IsMatch(Clipboard.GetText()))
            {
                new ApiAvatar { id = Clipboard.GetText() }.Get(
                    new Action<ApiContainer>(x =>
                                             {
                                                 GetSocialMenuInstance()
                                                     .transform.Find("Avatar")
                                                     .GetComponent<MonoBehaviour1PublicObReObBoVeBoGaVeStBoUnique>()
                                                     .field_Public_MonoBehaviourPublicSiGaRuGaAcRu4StAvObUnique_0
                                                     .field_Internal_ApiAvatar_0 = x.Model.Cast<ApiAvatar>();
                                                 GetSocialMenuInstance()
                                                     .transform.Find("Avatar")
                                                     .GetComponent<MonoBehaviour1PublicObReObBoVeBoGaVeStBoUnique>()
                                                     .field_Public_MonoBehaviourPublicSiGaRuGaAcRu4StAvObUnique_0
                                                     .field_Internal_ApiAvatar_0 = x.Model.Cast<ApiAvatar>();
                                                 GetSocialMenuInstance()
                                                     .transform.Find("Avatar")
                                                     .GetComponent<MonoBehaviour1PublicObReObBoVeBoGaVeStBoUnique>()
                                                     .ChangeToSelectedAvatar();
                                             }),
                    new Action<ApiContainer>(
                        x =>
                        {
                            MelonLogger.Msg(
                                $"Failed to change to avatar: {Clipboard.GetText()} | Error Message: {x.Error}");
                        }));
            }
            else
            {
                MelonLogger.Msg($"Invalid Avatar ID!");
            }
    }
}
}
