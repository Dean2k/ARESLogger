using System;
using TMPro;
using UnityEngine;
using VRC;

namespace AvatarLogger
{
    public class CustomNameplate : MonoBehaviour, IDisposable
    {
        private ImageThreeSlice _background;
        private TextMeshProUGUI _statsText;
        public Player Player;

        public CustomNameplate(IntPtr ptr) : base(ptr)
        {
        }

        public void Dispose()
        {
            _statsText.text = null;
            _statsText.OnDisable();
            _background.OnDisable();
            enabled = false;
        }

        private void Start()
        {
            if (enabled)
            {
                var stats = Instantiate(gameObject.transform.Find("Contents/Quick Stats"),
                    gameObject.transform.Find("Contents"));
                stats.parent = gameObject.transform.Find("Contents");
                stats.gameObject.SetActive(true);
                _statsText = stats.Find("Trust Text").GetComponent<TextMeshProUGUI>();
                _statsText.color = Color.white;
                stats.Find("Trust Icon").gameObject.SetActive(false);
                stats.Find("Performance Icon").gameObject.SetActive(false);
                stats.Find("Performance Text").gameObject.SetActive(false);
                stats.Find("Friend Anchor Stats").gameObject.SetActive(false);

                _background = gameObject.transform.Find("Contents/Main/Background").GetComponent<ImageThreeSlice>();

                _background._sprite = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform
                    .Find("Player Nameplate/Canvas/Nameplate/Contents/Main/Glow").GetComponent<ImageThreeSlice>()
                    ._sprite;
                _background.color = Color.black;
            }
        }

        private void Update()
        {
            if (enabled) _statsText.text = $"[{Player.GetPlatform()}] | " + $"[{Player.GetAvatarStatus()}]";
        }
    }
}