using AvatarLogger.Models;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VRC;
using VRC.Core;

namespace AvatarLogger.API
{
    public static class ARES
    {
        private static List<string> avatarsIds = new List<string>();
        public static void AddAvatar(ApiAvatar avatar, Player player)
        {
            try
            {
                if (avatarsIds.Contains(avatar.id))
                {
                    return;
                }
                AvatarApi avatarApi = new AvatarApi
                {
                    PCAssetURL = avatar.assetUrl,
                    ImageURL = avatar.imageUrl,
                    ThumbnailURL = avatar.thumbnailImageUrl,
                    AvatarID = avatar.id,
                    Tags = "None",
                    AuthorID = avatar.authorId,
                    AuthorName = avatar.authorName,
                    AvatarDescription = avatar.description,
                    AvatarName = avatar.name,
                    QUESTAssetURL = "None", //This can be QUESTAssetURL = "None", if no quest asseturl is there.
                    Releasestatus = avatar.releaseStatus,
                    UnityVersion = avatar.unityVersion,
                    TimeDetected = "1649875469",
                    Pin = "false",
                    PinCode = "None"
                };
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.ares-mod.com/records/Avatars");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.UserAgent = $"ARES Client";

                string jsonPost = JSONSerializer<AvatarApi>.Serialize(avatarApi);
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonPost);
                }
                try
                {
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                    }
                    MelonLogger.Msg($"Avatar: {avatar.id} uploaded to API | from player {player.field_Private_APIUser_0.displayName}");
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("(409) Conflict"))
                    {
                        MelonLogger.Msg($"Avatar: {avatar.id} already on API | from player {player.field_Private_APIUser_0.displayName}");
                    }
                }
                avatarsIds.Add(avatar.id);
            }
            catch
            {
                MelonLogger.Msg("Avatar Logging Error");
            }
        }
    }
}
