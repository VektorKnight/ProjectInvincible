using System;
using InvincibleEngine.UnitFramework.Enums;
using InvincibleEngine.UnitFramework.Interfaces;
using UnityEngine;

namespace InvincibleEngine.UnitFramework.Components {
    public class UnitBehavior : EntityBehavior, IUnit {
        
        // Unity Inspector
        [Header("Unit Settings")]
        [SerializeField] private UnitType _unitType;
        [SerializeField] private UnitCommand _supportedCommands;
        
        // Public Properties
        public UnitType UnitType => _unitType;
        public UnitCommand SupportedCommands => _supportedCommands;
        public UnitTeam UnitTeam { get; set; }
        public bool Invulnerable { get; private set; }
        public bool Selected { get; set; } = false;

        //Indicates unit selection
        private GameObject _selectionIndicator;

        // Base health of a unit
        public float Health = 100;

        // Denotes whether this unit can be built from somwhere, eventually this should be changed 
        // to a proper flag system that tells excactly where this unit can be made from
        public bool CanBeProduced = false;

        UnitType IUnit.UnitType {
            get {
                throw new NotImplementedException();
            }
        }       
        public virtual void GenerateResource() {

        }
        
        // Initialization
        public void Awake() {
            // Attempt to load the default selection indicator from resources directory
            var selectionPrefab = Resources.Load<GameObject>("Objects/Common/SelectionIndicator");
            
            // Instantiate the selection indicator as a child of this object
            if (selectionPrefab != null) {
                _selectionIndicator = Instantiate(selectionPrefab, transform.position, Quaternion.identity);
                //_selectionIndicator.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                _selectionIndicator.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
            }
            
            // Log an error if something goes boom boom
            if (_selectionIndicator == null)
                Debug.LogError("Failed to load default selection indicator prefab!");
            
            // Deactivate the indicator object if not null
            _selectionIndicator?.SetActive(false);
        }

        protected virtual void Update() {
            _selectionIndicator.transform.position = transform.position;
        }

        public virtual void ProcessCommand(UnitCommand cmd, object arg, bool overrideQueue = false) {

        }

        public override void OnSelected() {
            _selectionIndicator.SetActive(true);

        }

        public override void OnDeselected() {
            _selectionIndicator.SetActive(false);

        }

        public virtual void TakeDamage(float damage) {

        }
        
    }
}