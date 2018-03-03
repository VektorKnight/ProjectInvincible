using InvincibleEngine.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.UI_Elements {
    public class UI_LobbyOption : MonoBehaviour {
        public Text GLobbyName;
        public Text GPlayers;
        public int Index;
        public void Set(string name, Vector2 players, int index) {
            GLobbyName.text = name;
            GPlayers.text = $"{players.x}/{players.y}";
            Index = index;
        }
        public void OnSelect() {
            NetManager.Singleton.JoinLobby(Index);
        }
    }
}

