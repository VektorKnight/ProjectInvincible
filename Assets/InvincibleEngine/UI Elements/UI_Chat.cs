using InvincibleEngine.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.UI_Elements {
    public class UI_Chat : MonoBehaviour {
        public Text Chat;
        public InputField Input;
        public void PostChat() {
            Debug.Log("posting chat");
            NetManager.Singleton.SendLobbyChatMsg(new NetMessage.L_CHT(Input.text));
            Input.text = "";
        }
        private void Update() {
            Chat.text = NetManager.ChatLog.chat;
        }
    }
}
