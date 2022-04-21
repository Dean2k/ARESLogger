using System;
using System.Collections;
using System.IO;
using System.Text;
using MelonLoader;
using VRC;
using VRC.Core;

namespace AvatarLogger
{
    internal static class Logging
    {
        public static string FriendIDs;
        public static string MyLastAvatar = null;

        public static IEnumerator FetchFriends()
        {
            while (RoomManager.field_Internal_Static_ApiWorld_0 == null) yield return null;
            string[] pals = APIUser.CurrentUser.friendIDs.ToArray();
            foreach (var pal in pals) FriendIDs += $"{pal},";
        }

        public static void ExecuteLog(Player player, bool aviChange = false)
        {
            if (player == null) return;
            if (player.prop_ApiAvatar_0 == null) return;
            ApiAvatar apiAvatar = player.prop_ApiAvatar_0;
            if (!Main.Config.LogAvatars) return;
            if (!Main.Config.LogPublicAvatars)
                if (apiAvatar.releaseStatus == "public")
                {
                    if (Main.Config.ConsoleError && !aviChange)
                        MelonLogger.Msg(
                            $"Avatar {apiAvatar.name} was not logged, you have log public avatars disabled!");
                    return;
                }

            if (!Main.Config.LogPrivateAvatars)
                if (apiAvatar.releaseStatus == "private")
                {
                    if (Main.Config.ConsoleError && !aviChange)
                        MelonLogger.Msg(
                            $"Avatar {apiAvatar.name} was not logged, you have log private avatars disabled!");
                    return;
                }

            if (!Main.Config.LogOwnAvatars)
                if (APIUser.CurrentUser.id == apiAvatar.authorId)
                    if (MyLastAvatar != apiAvatar.id)
                    {
                        if (Main.Config.ConsoleError && !aviChange)
                            MelonLogger.Msg(
                                $"Your avatar {apiAvatar.name} was not logged, you have log own avatars disabled!");
                        return;
                    }

            if (!Main.Config.LogFriendsAvatars)
                if (FriendIDs.Contains(apiAvatar.authorId))
                {
                    if (Main.Config.ConsoleError && !aviChange)
                        MelonLogger.Msg(
                            $"{apiAvatar.authorName}'s avatar {apiAvatar.name} was not logged, they are a friend!");
                    return;
                }

            var AvatarFile = "GUI\\Log.txt";
            var AvatarFileIds = "GUI\\LogIds.txt";
            if (!File.Exists(AvatarFile))
                File.AppendAllText(AvatarFile, "Mod By ShrekamusChrist, LargestBoi & Yui\n");

            if (!File.Exists(AvatarFileIds))
                File.AppendAllText(AvatarFileIds, "Mod By ShrekamusChrist, LargestBoi & Yui\n");
            if (!HasAvatarId(AvatarFileIds, apiAvatar.id))
            {
                if (Main.Config.LogToConsole)
                    if (aviChange)
                        MelonLogger.Msg($"{player.prop_APIUser_0.displayName} changed into ({apiAvatar.name}|{apiAvatar.releaseStatus})!");
                File.AppendAllText(AvatarFileIds, apiAvatar.id + "\n");
                File.AppendAllLines(AvatarFile, new[]
                {
                    $"Time Detected:{((DateTimeOffset) DateTime.Now).ToUnixTimeSeconds().ToString()}",
                    $"Avatar ID:{apiAvatar.id}",
                    $"Avatar Name:{apiAvatar.name}",
                    $"Avatar Description:{apiAvatar.description}",
                    $"Author ID:{apiAvatar.authorId}",
                    $"Author Name:{apiAvatar.authorName}"
                });
                File.AppendAllLines(AvatarFile, new[]
                {
                    $"PC Asset URL:{apiAvatar.assetUrl}",
                    "Quest Asset URL:None",
                    $"Image URL:{apiAvatar.imageUrl}",
                    $"Thumbnail URL:{apiAvatar.thumbnailImageUrl}",
                    $"Unity Version:{apiAvatar.unityVersion}",
                    $"Release Status:{apiAvatar.releaseStatus}"
                });
                var rs = apiAvatar.releaseStatus;
                switch (rs)
                {
                    case "public":
                        Main.Pub += 1;
                        break;
                    case "private":
                        Main.Pri += 1;
                        break;
                }

                if (apiAvatar.tags.Count > 0)
                {
                    var builder = new StringBuilder();
                    builder.Append("Tags: ");
                    foreach (var tag in apiAvatar.tags) builder.Append($"{tag},");
                    File.AppendAllText(AvatarFile, builder.ToString().Remove(builder.ToString().LastIndexOf(",")));
                }
                else
                {
                    File.AppendAllText(AvatarFile, "Tags: None");
                }

                if (Main.Config.LogToConsole)
                    MelonLogger.Msg($"Logged: {player.prop_APIUser_0.displayName}'s avatar ({apiAvatar.name}|{apiAvatar.releaseStatus})!");
                File.AppendAllText(AvatarFile, "\n\n");
            }
        }

        public static bool HasAvatarId(string avatarFile, string avatarId)
        {
            var lines = File.ReadLines(avatarFile);
            foreach (var line in lines)
                if (line.Contains(avatarId))
                    return true;

            return false;
        }

        public static void ExecuteLogWorld(ApiWorld worldTable)
        {
            if (Main.Config.LogWorlds)
            {
                var avatarFile = "GUI\\LogWorld.txt";
                var avatarFileIds = "GUI\\LogWorldIds.txt";
                if (!File.Exists(avatarFile))
                    File.AppendAllText(avatarFile, "Mod By ShrekamusChrist, LargestBoi & Yui\n");

                if (!File.Exists(avatarFileIds))
                    File.AppendAllText(avatarFileIds, "Mod By ShrekamusChrist, LargestBoi & Yui\n");
                if (!HasAvatarId(avatarFileIds, worldTable.id))
                {
                    File.AppendAllText(avatarFileIds, worldTable.id + "\n");
                    File.AppendAllLines(avatarFile, new[]
                    {
                        $"Time Detected:{((DateTimeOffset) DateTime.Now).ToUnixTimeSeconds().ToString()}",
                        $"World ID:{worldTable.id}",
                        $"World Name:{worldTable.name}",
                        $"World Description:{worldTable.description}",
                        $"Author ID:{worldTable.authorId}",
                        $"Author Name:{worldTable.authorName}"
                    });

                    File.AppendAllLines(avatarFile, new[]
                    {
                        $"PC Asset URL:{worldTable.assetUrl}",
                        $"Image URL:{worldTable.imageUrl}",
                        $"Thumbnail URL:{worldTable.thumbnailImageUrl}",
                        $"Unity Version:{worldTable.unityVersion}",
                        $"Release Status:{worldTable.releaseStatus}"
                    });
                    if (worldTable.tags.Count > 0)
                    {
                        var builder = new StringBuilder();
                        builder.Append("Tags: ");
                        foreach (var tag in worldTable.tags) builder.Append($"{tag},");
                        File.AppendAllText(avatarFile, builder.ToString().Remove(builder.ToString().LastIndexOf(",")));
                    }
                    else
                    {
                        File.AppendAllText(avatarFile, "Tags: None");
                    }

                    if (Main.Config.LogToConsole)
                        MelonLogger.Msg(
                            $"Logged: {worldTable.authorName}'s World ({worldTable.name}|{worldTable.releaseStatus})!");
                    File.AppendAllText(avatarFile, "\n\n");
                }
            }
        }
    }
}