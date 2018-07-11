using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SteamNet {
    public class UIMessage : MonoBehaviour {

        public static UIMessage GlobalMessage { get { return GameObject.Find("Message").GetComponent<UIMessage>(); } }

        public GameObject Holder;
        public Text DisplayText;

        public void DisplayMessage(string message) {
            Holder.SetActive(true);
            DisplayText.text = message;

        }

        public void HideMessage() {
            Holder.SetActive(false);
        }
    }
}
