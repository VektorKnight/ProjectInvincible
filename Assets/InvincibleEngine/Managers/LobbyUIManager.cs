using UnityEngine;

namespace InvincibleEngine.Managers {
    public class LobbyUIManager : MonoBehaviour {
        //Singleton
        private static LobbyUIManager _Singleton = null;
        public static LobbyUIManager Singleton { get { if (_Singleton == null) { _Singleton = GameObject.Find("Managers").GetComponent<LobbyUIManager>(); } return _Singleton; } private set { } }

        private void Start() {
        
        }

    }
}
