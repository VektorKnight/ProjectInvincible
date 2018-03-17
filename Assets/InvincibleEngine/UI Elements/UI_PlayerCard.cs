using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.UI_Elements {
	public class UI_PlayerCard : MonoBehaviour {
        [SerializeField]
		public Text NameText;
        public Image Team;
        public RawImage PlayerCard;

        public  void SetTeamColor(Color color) {
            Team.color = color;
        }

        public void SetProfileImage(Texture2D image) {
            PlayerCard.texture = image;
        }
	}
}
