using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace AvatarLogger
{
    public class CustomNameplate : MonoBehaviour, IDisposable
    {
        public VRC.Player player;
        private TextMeshProUGUI statsText;
        private ImageThreeSlice background;

        public CustomNameplate(IntPtr ptr) : base(ptr)
        {
        }

        void Start()
        {
            if (this.enabled)
            {
                Transform stats = UnityEngine.Object.Instantiate<Transform>(this.gameObject.transform.Find("Contents/Quick Stats"), this.gameObject.transform.Find("Contents"));
                stats.parent = this.gameObject.transform.Find("Contents");
                stats.gameObject.SetActive(true);
                statsText = stats.Find("Trust Text").GetComponent<TextMeshProUGUI>();
                statsText.color = Color.white;
                stats.Find("Trust Icon").gameObject.SetActive(false);
                stats.Find("Performance Icon").gameObject.SetActive(false);
                stats.Find("Performance Text").gameObject.SetActive(false);
                stats.Find("Friend Anchor Stats").gameObject.SetActive(false);

                background = this.gameObject.transform.Find("Contents/Main/Background").GetComponent<ImageThreeSlice>();

                background._sprite = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.Find("Player Nameplate/Canvas/Nameplate/Contents/Main/Glow").GetComponent<ImageThreeSlice>()._sprite;
                background.color = Color.black;
            }
        }

        void Update()
        {
            if (this.enabled)
            {
                statsText.text = $"[{player.GetPlatform()}] | " + $"[{ player.GetAvatarStatus()}]";
            }
        }

        public void Dispose()
        {
            statsText.text = null;
            statsText.OnDisable();
            background.OnDisable();
            this.enabled = false;
        }
    }
}
