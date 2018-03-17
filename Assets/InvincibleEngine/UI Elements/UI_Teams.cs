using System;
using System.Collections;
using InvincibleEngine.Managers;
using UnityEngine;

namespace InvincibleEngine.UI_Elements {
    public class UI_Teams : MonoBehaviour {

        public GameObject Teams;
        public GameObject PlayerCardPrefab;

        int CurrentElements = 0;

        //remove all player cards, spawn new ones according to whos in the lobby
        void Update() {
            if (CurrentElements != NetManager.Singleton.LobbyMembers.Count) {

                //destroy all current ones
                foreach (Transform n in Teams.transform) {
                    Destroy(n.gameObject);
                }

                //create new player cards
                try {
                    foreach (NetManager.LobbyMember n in NetManager.Singleton.LobbyMembers) {
                        UI_PlayerCard x = Instantiate(PlayerCardPrefab, Teams.transform).GetComponent<UI_PlayerCard>();
                        x.NameText.text = n.Name;

                    }
                }
                catch(NullReferenceException) {
                    Debug.Log("List of players is null, possible JSON error");
                }
                //reset active sum
                CurrentElements = NetManager.Singleton.LobbyMembers.Count;
            }
        }
    }
}
