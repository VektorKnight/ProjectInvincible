using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;
using InvincibleEngine;
using SteamNet;
using UnityEngine;

namespace VektorLibrary.EntityFramework.Components {
    public abstract class EntityBehavior : MonoBehaviour {

        // Property: Registered
        public bool Registered { get; private set; }

        // Property: Terminating
        public bool Terminating { get; private set; }

        //Owner of object, -1 is the empty player
        public CSteamID PlayerOwner;

        //Easy access to the local lobby data
        public LobbyData CurrentLobbyData {
            get { return SteamNetManager.Instance.CurrentlyJoinedLobby; }
        }

        // Unity Initialization
        public virtual void Start() {
            // Exit if already registered
            if (Registered) return;

            // Register with the Entity Manager
            EntityManager.RegisterBehavior(this);
        }

        // Unity Destroy
        protected virtual void OnDestroy() {
            EntityManager.UnregisterBehavior(this);
        }

        // Behavior regisration callback
        public virtual void OnRegister() {
            Registered = true;
        }

        //Economy Update
        public virtual void OnEconomyUpdate(float fixedDelta, bool isHost) { }

        // Entity update callback   
        public virtual void OnSimUpdate(float fixedDelta, bool isHost) { }

        // Render update callback
        public virtual void OnRenderUpdate(float renderDelta) { }

        // Termination
        public virtual void Terminate() {
            Terminating = true;
            Destroy(this);
        }
    }
}