using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.UI_Elements {
	public class UI_PlayerCard : MonoBehaviour {
        [SerializeField]
		public Text NameText;
        public Image Team;
        public Texture2D PlayerCard;

        public  void SetTeamColor(Color color) {
            Team.color = color;
        }
	}
}
