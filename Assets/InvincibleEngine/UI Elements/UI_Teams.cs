using System.Collections;
using InvincibleEngine.Managers;
using UnityEngine;

namespace InvincibleEngine.UI_Elements {
    public class UI_Teams : MonoBehaviour {

        public GameObject Teams;
        public GameObject PlayerCardPrefab;
        void Start() {
            StartCoroutine("UIUpdate");
        }


        //remove all player cards, spawn new ones according to whos in the lobby
        IEnumerator UIUpdate() {
            while (true) {

                //destroy all current ones
                foreach (Transform n in Teams.transform) {
                    Destroy(n.gameObject);
                }
                

                foreach (NetManager.LobbyMember n in NetManager.Singleton.LobbyMembers) {
                    UI_PlayerCard x = Instantiate(PlayerCardPrefab, Teams.transform).GetComponent<UI_PlayerCard>();
                    x.NameText.text = n.Name;
                    if(NetManager.Singleton.IsLobbyOwner) {

                    }
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
