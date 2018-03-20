using InvincibleEngine.Managers;
using UnityEngine.UI;
using UnityEngine;

namespace InvincibleEngine.UI_Elements {
    public class UI_LobbySettings : MonoBehaviour {

        public Text CountdownTimer;

        public void CreateLobby() {
            NetManager.Singleton.CreateLobby();   
        }

        public void LeaveLobby() {
            NetManager.Singleton.LeaveLobby();
        }

        /// <summary>
        /// If host, request to start game countdown, if client toggle ready
        /// </summary>
        public void OnReady() {
            if(NetManager.Singleton.NetworkState== NetManager.ENetworkState.Connected) {
                NetManager.Singleton.SendLobbyChatMsg(new NetMessage.L_RDY());
            }

            if(NetManager.Singleton.NetworkState== NetManager.ENetworkState.Hosting) {
                NetManager.Singleton.LaunchGame();
            }
        }

        public void Update() {
            CountdownTimer.text = NetManager.Singleton.LaunchGameCountdown.ToString("0.00");
        }
    }
}
