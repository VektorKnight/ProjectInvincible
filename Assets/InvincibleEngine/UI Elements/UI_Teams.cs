using System;
using System.Collections;
using InvincibleEngine.Managers;
using UnityEngine;

namespace InvincibleEngine.UI_Elements {
    public class UI_Teams : MonoBehaviour {

        public GameObject Teams;
        public GameObject PlayerCardPrefab;

        private void Start() {
            StartCoroutine(UpdateCards());
        }

        //remove all player cards, spawn new ones according to whos in the lobby
        IEnumerator UpdateCards() {
            while (true) {
                //destroy all current ones
                foreach (Transform n in Teams.transform) {
                    Destroy(n.gameObject);
                }

                //create new player cards
                try {
                    foreach (LobbyMember n in NetManager.Instance.LobbyMembers) {
                        UI_PlayerCard x = Instantiate(PlayerCardPrefab, Teams.transform).GetComponent<UI_PlayerCard>();
                        x.NameText.text = n.Name;
                        x.SetTeamColor(NetManager.Instance.TeamColors[n.Team]);
                        x.SetProfileImage(NetManager.Instance.GetSmallAvatar(n.SteamID));

                        Debug.Log($"Making player card with status {n.Ready}");
                        x.SetReady(n.Ready, n.IsHost);
                    }
                }
                catch (NullReferenceException) {
                    Debug.Log("List of players is null, possible JSON error");
                }

                yield return new WaitForSeconds(1);
            }
           
        }
    }
}
