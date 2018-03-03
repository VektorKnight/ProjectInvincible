using InvincibleEngine.Managers;
using UnityEngine;

namespace InvincibleEngine.UI_Elements {
    public class UI_LobbySettings : MonoBehaviour {

        public void CreateLobby() {
            NetManager.Singleton.CreateLobby();   
        }
    }
}
