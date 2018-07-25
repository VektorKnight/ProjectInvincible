using System;
using System.Collections;
using InvincibleEngine.CameraSystem;
using InvincibleEngine.UnitFramework.DataTypes;
using InvincibleEngine.UnitFramework.Enums;
using InvincibleEngine.UnitFramework.Interfaces;
using InvincibleEngine.UnitFramework.Utility;
using VektorLibrary.EntityFramework.Components;
using UnityEngine;
using VektorLibrary.Utility;
using Outline = cakeslice.Outline;
using Random = UnityEngine.Random;

namespace InvincibleEngine.UnitFramework.Components {
    [RequireComponent(typeof(Outline))]
    [RequireComponent(typeof(LineRenderer))]
    public class UnitBehavior : EntityBehavior, IUnit {

        // Constant: Team Layers Start/End
        public static readonly int[] TeamLayerBounds = {11, 18};

        // Unity Inspector        
        [Header("General Settings")]
        [SerializeField] private UnitType _unitType;
        [SerializeField] private Sprite _iconSprite;
        [SerializeField] public BuildOption[] BuildOptions;

        [Header("Target Acquisition")] 
        [SerializeField] protected bool AutoAcquireTargets;
        [SerializeField] protected float ScanRadius = 50f;
        [SerializeField] protected LayerMask ScanLayers;
        [SerializeField] protected Vector2 ScanIntervalRange = new Vector2(0.2f, 0.4f);
        
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
        public Team UnitTeam { get; private set; }
        public Color UnitColor { get; private set; }
        public Sprite IconSprite => _iconSprite;
        public UnitActions SupportedCommands { get; protected set; }
        public bool Invulnerable { get; private set; }
        public bool Selected { get; private set; }

        // Denotes whether this unit can be built from somwhere, eventually this should be changed 
        // to a proper flag system that tells excactly where this unit can be made from
        public bool CanBeProduced;  
        
        public virtual void GenerateResource() {}       
        
        // Initialization
        public override void OnRegister() {
            // Reference required components
            UnitRenderer = GetComponentInChildren<MeshRenderer>();
            LineRenderer = GetComponent<LineRenderer>();
            SelectionIndicator = GetComponent<Outline>();
            
            // Initialize line renderer
            LineRenderer.useWorldSpace = true;
            LineRenderer.positionCount = 2;
            LineRenderer.startColor = TeamColor.GetTeamColor(UnitTeam);
            LineRenderer.endColor = TeamColor.GetTeamColor(UnitTeam);
            
            // Fetch team color from team
            UnitColor = TeamColor.GetTeamColor(UnitTeam);
            
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
                Icon.Initialize(_iconSprite, UnitColor);
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
            
            // Calculate the scan layers based on the unit team
            CalculateLayers();
            
            // Start the scanning routine if necessary
            if (AutoAcquireTargets)
                StartCoroutine(nameof(ScanForTargets));
            
            // Call base method
            base.OnRegister();
        }
        
        // Set this unit's team and update related objects
        public virtual void SetTeam(Team team) {
            UnitTeam = team;
            UnitColor = TeamColor.GetTeamColor(team);
            Icon?.SetColor(UnitColor);
            CalculateLayers();
        }
        
        // Calculate the layers and masks for this unit
        protected virtual void CalculateLayers() {
            // Make sure this unit's layer is set to match it's team
            gameObject.layer = TeamLayerBounds[0] + (int) UnitTeam;
            
            // Calculate the scan layers for target acquisition
            foreach (var team in Enum.GetValues(typeof(Team))) {
                // Skip this unit's team
                if ((Team) team == UnitTeam) continue;
                
                // Set the appropriate bit flags
                ScanLayers = ScanLayers | (int)Mathf.Pow(2, TeamLayerBounds[0] + (int) team);
            }
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

                LineRenderer.enabled = DebugReadout.ShowTargeting;
				
                // Make sure the current target is alive
                if (CurrentTarget != null) {
                    // Update line renderers if necessary
                    if (DebugReadout.ShowTargeting) {
                        LineRenderer.SetPosition(0, transform.position);
                        LineRenderer.SetPosition(1, CurrentTarget.transform.position);
                    }

                    // Calculate sqr distance to target
                    var sqrTargetDistance = Vector3.SqrMagnitude(transform.position - CurrentTarget.transform.position);
                    
                    // Check that the target is within range
                    if (sqrTargetDistance > SqrScanRadius) {
                        // Discard the current target if it's out of range
                        CurrentTarget = null;
						
                        // Set waiting flag to true
                        WaitingForTarget = true;
                    }
                }
                else {
                    if (DebugReadout.ShowTargeting) {
                        LineRenderer.SetPosition(0, Vector3.zero);
                        LineRenderer.SetPosition(1, Vector3.zero);
                    }
                }
				
                yield return ScanInterval;
            }
        }
        
        // Render Update Callback
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
        
        // Called when this unit is selected
        public virtual void OnSelected() {
            // Show selection indicator if icons are not rendered
            SelectionIndicator.enabled = true;
            
            // Set icon state to selected
            Icon.SetSelected(true);
            
            // Set selected flag
            Selected = true;
        }
        
        // Called when this unit is deselected
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