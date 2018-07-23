using System;
using InvincibleEngine.CameraSystem;
using InvincibleEngine.UnitFramework.DataTypes;
using InvincibleEngine.UnitFramework.Enums;
using InvincibleEngine.UnitFramework.Interfaces;
using InvincibleEngine.UnitFramework.Utility;
using VektorLibrary.EntityFramework.Components;
using UnityEngine;
using Outline = cakeslice.Outline;

namespace InvincibleEngine.UnitFramework.Components {
    public class UnitBehavior : EntityBehavior, IUnit {
        
        // Unity Inspector
        [Header("Unit Settings")]
        [SerializeField] private UnitType _unitType;
        [SerializeField] private UnitIcon _unitIcon;
        [SerializeField] public BuildOption[] BuildOptions;
        
        // Protected: Unit Icon
        protected UnitIcon UnitIcon;
        
        // Protected: Component References
        protected Renderer UnitRenderer;
        
        // Protected: Command Processing
        protected CommandParser CommandParser = new CommandParser();
        
        // Public Properties
        public UnitType UnitType => _unitType;
        public UnitActions SupportedCommands { get; protected set; }
        public UnitTeam UnitTeam { get; set; }
        public bool Invulnerable { get; private set; }
        public bool Selected { get; private set; }

        // Indicates unit selection
        private Outline _selectionIndicator;

        // Base health of a unit
        public float Health = 100;

        // Denotes whether this unit can be built from somwhere, eventually this should be changed 
        // to a proper flag system that tells excactly where this unit can be made from
        public bool CanBeProduced;

        UnitType IUnit.UnitType {
            get {
                throw new NotImplementedException();
            }
        }       
        public virtual void GenerateResource() {

        }       

        // Image used for unit icon
        public Sprite Icon;
        
        // Initialization
        public override void OnRegister() {
            // Reference required components
            UnitRenderer = GetComponent<Renderer>();
            
            // Reference required outline component
            _selectionIndicator = GetComponent<Outline>();
            
            // Instantiate this unit's icon if possible
            if (_unitIcon != null) {
                UnitIcon = Instantiate(_unitIcon);
                UnitIcon.Initialize();
                InvincibleCamera.AppendUnitIcon(UnitIcon);
                UnitIcon.SetSelected(false);
            }
            
            // Log an error if something goes boom boom
            if (_selectionIndicator == null)
                Debug.LogError("Failed to load default selection indicator prefab!");
            
            // Deactivate the indicator object if not null
            _selectionIndicator.enabled = false;
            
            // Call base method
            base.OnRegister();
        }

        public override void OnRenderUpdate(float deltaTime) {
            // Determine if this object is on screen or not
            if (UnitIcon != null)
                UnitIcon.SetScreenPosition(InvincibleCamera.GetScreenPosition(transform.position));

            if (GeometryUtility.TestPlanesAABB(InvincibleCamera.FrustrumPlanes, UnitRenderer.bounds)) {
                InvincibleCamera.VisibleObjects.Add(this);
                UnitIcon?.SetRender(true);
            }
            else {
                InvincibleCamera.VisibleObjects.Remove(this);
                UnitIcon?.SetRender(false);
            }
        }

        public virtual void OnSelected() {
            // Show selection indicator if icons are not rendered
            _selectionIndicator.enabled = true;
            
            // Set icon state to selected
            UnitIcon.SetSelected(true);
            
            // Set selected flag
            Selected = true;
        }

        public virtual void OnDeselected() {
            // Hide selection indicator
            _selectionIndicator.enabled = false;
            
            // Set icon state to unselected
            UnitIcon.SetSelected(false);
            
            // Set selected flag
            Selected = false;
        }

        public virtual void TakeDamage(float damage) {

        }

        public void ProcessCommand(UnitCommand command) {
            CommandParser.ProcessCommand(command);
        }
    }
}