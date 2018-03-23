using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.UI_Elements {
	public class UI_PlayerCard : MonoBehaviour {
        [SerializeField]
		public Text NameText;
        public Image Team;
        public RawImage PlayerCard;
        public Image PanelHolder;
        public Color StandardColor;
        public Color ReadyColor;

        private void Start() {
            //PanelHolder.color = StandardColor;
        }

        public  void SetTeamColor(Color color) {
            Team.color = color;
        }

        public void SetProfileImage(Texture2D image) {
            PlayerCard.texture = image;
        }

        public void SetReady(bool toggle) {
            Debug.Log($"Setting color to {toggle}");
            if(toggle) {
                PanelHolder.color = ReadyColor;
            }
            else {
                PanelHolder.color = StandardColor;
            }
        }
	}
}
