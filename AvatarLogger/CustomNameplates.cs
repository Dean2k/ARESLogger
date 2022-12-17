using Photon.Realtime;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using VRC;
using Object = UnityEngine.Object;

namespace AvatarLogger
{
    public class CustomNameplate : MonoBehaviour, IDisposable
    {
        //private ImageThreeSlice _background;
        private TextMeshProUGUI _statsText;
        public VRC.Player Player;

        public CustomNameplate(IntPtr ptr) : base(ptr)
        {
        }

        public void Dispose()
        {
            _statsText.text = null;
            _statsText.OnDisable();
            //_background.OnDisable();
            enabled = false;
        }

        private void Start()
        {
            if (enabled)
            {
                var namePlateManager = Player._vrcplayer.field_Public_PlayerNameplate_0;//Object.FindObjectsOfType<PlayerNameplate>().FirstOrDefault(x=>x.prop_VRCPlayer_0._player == Player);
                var PlateTemplate = namePlateManager.field_Public_GameObject_5.transform;
                var stats = Instantiate(PlateTemplate,PlateTemplate);
                stats.parent = namePlateManager.transform;
                stats.gameObject.SetActive(true);
                _statsText = stats.Find("Trust Text").GetComponent<TextMeshProUGUI>();
                _statsText.color = Color.white;
                stats.Find("Trust Icon").gameObject.SetActive(false);
                stats.Find("Performance Icon").gameObject.SetActive(false);
                stats.Find("Performance Text").gameObject.SetActive(false);
                stats.Find("Friend Anchor Stats").gameObject.SetActive(false);

                //_background = gameObject.transform.Find("Contents/Main/Background").GetComponent<ImageThreeSlice>();

                //_background._sprite = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform
                //    .Find("Player Nameplate/Canvas/Nameplate/Contents/Main/Glow").GetComponent<ImageThreeSlice>()
                //    ._sprite;
                //_background.color = Color.black;
            }
        }

        private void Update()
        {
            if (enabled) _statsText.text = $"[{Player.GetPlatform()}] | " + $"[{Player.GetAvatarStatus()}]";
        }
    }
}