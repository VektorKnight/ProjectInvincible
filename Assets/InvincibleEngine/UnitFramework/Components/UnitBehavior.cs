using System;
using System.Collections;
using InvincibleEngine.CameraSystem;
using InvincibleEngine.UnitFramework.DataTypes;
using InvincibleEngine.UnitFramework.Enums;
using InvincibleEngine.UnitFramework.Interfaces;
using InvincibleEngine.UnitFramework.Utility;
using VektorLibrary.EntityFramework.Components;
using UnityEngine;
using Outline = cakeslice.Outline;
using Random = UnityEngine.Random;

namespace InvincibleEngine.UnitFramework.Components {
    [RequireComponent(typeof(Outline))]
    [RequireComponent(typeof(LineRenderer))]
    public class UnitBehavior : EntityBehavior, IUnit {
        
        // Unity Inspector
        [Header("TEMP DEBUG")] 
        [SerializeField] private UnitTeam _tempTeam;
        [SerializeField] private Color _tempColor;
        
        [Header("General Settings")]
        [SerializeField] private UnitType _unitType;
        [SerializeField] private Sprite _iconSprite;
        [SerializeField] public BuildOption[] BuildOptions;

        [Header("Target Acquisition")] 
        [SerializeField] protected bool AutoAcquireTargets;
        [SerializeField] protected float ScanRadius = 50f;
        [SerializeField] protected LayerMask ScanLayers;
        [SerializeField] protected Vector2 ScanIntervalRange = new Vector2(0.2f, 0.4f);

        [Header("Debugging")] 
        [SerializeField] protected bool ShowTargetingLine;
        
        // Protected: Unit Icon
        protected UnitIcon Icon;
        
        // Protected: Component References
        protected Renderer UnitRenderer;
        protected LineRenderer LineRenderer;
        protected Outline SelectionIndicator;
        
        // Protected: Command Processing
        protected CommandParser CommandParser = new CommandParser();
        
        // Protected: Target Acquisition
        protected TargetScanner Scanner;
        protected WaitForSeconds ScanInterval;
        protected UnitBehavior CurrentTarget;
        protected bool WaitingForTarget = true;
        protected float SqrScanRadius;
        
        // Public Properties
        public UnitType UnitType => _unitType;
        public UnitActions SupportedCommands { get; protected set; }
        public UnitTeam UnitTeam => _tempTeam;
        public bool Invulnerable { get; private set; }
        public bool Selected { get; private set; }
        
        // Base health of a unit
        public float Health = 100;

        // Denotes whether this unit can be built from somwhere, eventually this should be changed 
        // to a proper flag system that tells excactly where this unit can be made from
        public bool CanBeProduced;  
        
        public virtual void GenerateResource() {}       
        
        // Initialization
        public override void OnRegister() {
            // Reference required components
            UnitRenderer = GetComponent<Renderer>();
            LineRenderer = GetComponent<LineRenderer>();
            SelectionIndicator = GetComponent<Outline>();
            
            // Initialize line renderer
            LineRenderer.useWorldSpace = true;
            LineRenderer.positionCount = 2;
            
            // Construct this unit's icon if possible
            if (_iconSprite != null) {
                // Declare template reference
                UnitIcon template;
                
                // Load the appropriate template for the unit type
                switch (_unitType) {
                    case UnitType.Structure:
                        template = Resources.Load<UnitIcon>("Objects/Templates/Icon-Structure");
                        break;
                    case UnitType.Special:
                        template = Resources.Load<UnitIcon>("Objects/Templates/Icon-Special");
                        break;
                    default:
                        template = Resources.Load<UnitIcon>("Objects/Templates/Icon-Unit");
                        break;
                }
                
                // Instantiate and initialize the unit icon
                Icon = Instantiate(template);
                Icon.Initialize(_iconSprite, _tempColor);
                InvincibleCamera.AppendUnitIcon(Icon);
                Icon.SetSelected(false);
            }
            
            // Deactivate the indicator object if not null
            if (SelectionIndicator != null)
                SelectionIndicator.enabled = false;
            
            // Initialize the target scanner and supporting objects
            Scanner = new TargetScanner(ScanLayers);
            ScanInterval = new WaitForSeconds(Random.Range(ScanIntervalRange.x, ScanIntervalRange.y));
            SqrScanRadius = ScanRadius * ScanRadius;
            
            // Start the scanning routine if necessary
            if (AutoAcquireTargets)
                StartCoroutine(nameof(ScanForTargets));
            
            // Call base method
            base.OnRegister();
        }
        
        // Time slicing coroutine
        protected virtual IEnumerator ScanForTargets() {
            while (true) {
                // If we are waiting for a target, initiate a scan
                if (WaitingForTarget) {
                    // Scan for targets in range
                    CurrentTarget = Scanner.ScanForNearestTarget(transform.position, 50f);
					
                    // Set waiting flag to true if target found
                    WaitingForTarget = CurrentTarget == null;
                }
				
                // Make sure the current target is alive
                if (CurrentTarget != null) {
					
                    // Update line renderers if necessary
                    if (ShowTargetingLine) {
                        LineRenderer.SetPosition(0, transform.position);
                        LineRenderer.SetPosition(1, CurrentTarget.transform.position);
                    }

                    // Make sure the current target is within range
                    var sqrTargetDistance = Vector3.SqrMagnitude(transform.position - CurrentTarget.transform.position);
                    if (sqrTargetDistance > SqrScanRadius) {
                        // Discard the current target if it's out of range
                        CurrentTarget = null;
						
                        // Set waiting flag to true
                        WaitingForTarget = true;
                    }
                }
                else {
                    LineRenderer.SetPosition(0, Vector3.zero);
                    LineRenderer.SetPosition(1, Vector3.zero);
                }
				
                yield return ScanInterval;
            }
        }

        public override void OnRenderUpdate(float deltaTime) {
            // Determine if this object is on screen or not
            if (Icon != null)
                Icon.SetScreenPosition(InvincibleCamera.GetScreenPosition(transform.position));

            if (GeometryUtility.TestPlanesAABB(InvincibleCamera.FrustrumPlanes, UnitRenderer.bounds)) {
                InvincibleCamera.VisibleObjects.Add(this);
                Icon?.SetRender(true);
            }
            else {
                InvincibleCamera.VisibleObjects.Remove(this);
                Icon?.SetRender(false);
            }
        }

        public virtual void OnSelected() {
            // Show selection indicator if icons are not rendered
            SelectionIndicator.enabled = true;
            
            // Set icon state to selected
            Icon.SetSelected(true);
            
            // Set selected flag
            Selected = true;
        }

        public virtual void OnDeselected() {
            // Hide selection indicator
            SelectionIndicator.enabled = false;
            
            // Set icon state to unselected
            Icon.SetSelected(false);
            
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