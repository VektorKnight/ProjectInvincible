using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InvincibleEngine.Managers;
using UnityEngine;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;

namespace InvincibleEngine.UI_Elements {
    public class UI_LobbyList : MonoBehaviour {

        public GameObject GLobbyOptionPrefab;
        public GameObject GLobbyListHolder;
        List<CSteamID> previousList = new List<CSteamID>();

        void Start () {
            StartCoroutine("UpdatelobbyList");
        }
        IEnumerator UpdatelobbyList() {
            while(true) {
               
                    //clear all current lobbies            
                    foreach (Transform n in GLobbyListHolder.transform) {
                        Destroy(n.gameObject);
                    }

                    //populate lobby list
                    for (int i = 0; i <NetManager.Instance.lobbyIDS.Count; i++) {
                        GameObject x = Instantiate(GLobbyOptionPrefab, GLobbyListHolder.transform);
                        x.GetComponent<UI_LobbyOption>().Set(NetManager.Instance.lobbyIDS[i].ToString(), new Vector2(1, 10), i);
                    }

                    //set previous list to current for changes
                    previousList =NetManager.Instance.lobbyIDS;
                
                yield return new WaitForSeconds(1);
            }
        }
    }
}
