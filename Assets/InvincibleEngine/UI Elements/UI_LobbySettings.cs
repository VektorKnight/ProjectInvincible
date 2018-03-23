using InvincibleEngine.Managers;
using UnityEngine.UI;
using UnityEngine;

namespace InvincibleEngine.UI_Elements {
    public class UI_LobbySettings : MonoBehaviour {

        public Text CountdownTimer;

        public void CreateLobby() {
           NetManager.Instance.CreateLobby();   
        }

        public void LeaveLobby() {
           NetManager.Instance.LeaveLobby();
            
        }

        /// <summary>
        /// If host, request to start game countdown, if client toggle ready
        /// </summary>
        public void OnReady() {
            if(NetManager.Instance.NetworkState== NetManager.ENetworkState.Connected) {
               NetManager.Instance.SendLobbyChatMsg(new NetMessage.L_RDY());
            }

            if(NetManager.Instance.NetworkState== NetManager.ENetworkState.Hosting) {
               NetManager.Instance.LaunchGame();
            }
        }

        public void Update() {
            CountdownTimer.text =NetManager.Instance.GameOptions.Timer.ToString("0.0");
        }
    }
}
