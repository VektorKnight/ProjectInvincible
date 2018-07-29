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
    /// <summary>
    /// Base class for all unit behaviors.
    /// Base methods for this class should be called first in any overrides.
    /// </summary>
    [RequireComponent(typeof(Outline))]
    [RequireComponent(typeof(LineRenderer))]
    public class UnitBehavior : EntityBehavior, IUnit {

        // Constant: Team Layers Start/End
        public static readonly int[] TeamLayerBounds = {11, 18};

        // Unity Inspector        
        [Header("General Settings")]
        [SerializeField] private UnitType _unitType;
        [SerializeField] protected float Health = 100f;
        [SerializeField] public BuildOption[] BuildOptions;
        
        [Header("Gameplay UI Elements")]
        [SerializeField] private Sprite _iconSprite;
        [SerializeField] private Vector2Int _healthBarSize = new Vector2Int(48, 4);

        [Header("Target Acquisition")] 
        [SerializeField] protected bool AutoAcquireTargets;
        [SerializeField] protected float ScanRadius = 50f;
        [SerializeField] protected Vector2 ScanIntervalRange = new Vector2(0.2f, 0.4f);

        [Header("Destruction Aesthetics")] 
        [SerializeField] protected ParticleSystem DeathEffect;
        
        // Protected: Unit Icon
        protected UnitScreenElement Icon;
        protected UnitScreenElement HealthBar;
        
        // Protected: Component References
        protected MeshRenderer UnitRenderer;
        protected LineRenderer LineRenderer;
        protected Outline SelectionIndicator;
        
        // Protected: Command Processing
        protected CommandParser CommandParser = new CommandParser();
        
        // Protected: Target Acquisition
        protected WaitForSeconds ScanInterval;
        protected bool WaitingForTarget = true;
        protected float SqrScanRadius;
        
        // Properties: Unit Metadata
        public UnitType UnitType => _unitType;
        public Team UnitTeam { get; protected set; }
        public Color UnitColor { get; protected set; }
        public float CurrentHealth { get; protected set; }
        public Sprite IconSprite => _iconSprite;
        public UnitActions SupportedCommands { get; protected set; }
        
        // Properties: Target Acquisition
        public UnitBehavior CurrentTarget { get; protected set; }
        public LayerMask ScanLayers { get; protected set; }
        public float WeaponRange => ScanRadius;
        
        // Properties: Unit State
        public bool Invulnerable { get; protected set; }
        public bool Selected { get; protected set; }
        public bool Dying { get; protected set; }

        // Denotes whether this unit can be built from somewhere, eventually this should be changed 
        // to a proper flag system that tells exactly where this unit can be made from
        public bool CanBeProduced;  
        
        public virtual void GenerateResource() {}       
        
        // Initialization
        public override void OnRegister() {
            // Reference required components
            UnitRenderer = GetComponent<MeshRenderer>();
            LineRenderer = GetComponent<LineRenderer>();
            SelectionIndicator = GetComponent<Outline>();
            
            // Initialize line renderer
            LineRenderer.useWorldSpace = true;
            LineRenderer.positionCount = 2;
            LineRenderer.startColor = TeamColor.GetTeamColor(UnitTeam);
            LineRenderer.endColor = TeamColor.GetTeamColor(UnitTeam);
            
            // Fetch team color from team
            UnitColor = TeamColor.GetTeamColor(UnitTeam);
            UnitRenderer.material.SetColor("_TeamColor", UnitColor);
            
            // Construct this unit's icon if possible
            if (_iconSprite != null) {
                // Declare template reference
                UnitScreenElement template;
                
                // Load the appropriate template for the unit type
                switch (_unitType) {
                    case UnitType.Structure:
                        template = Resources.Load<UnitScreenElement>("Objects/Templates/Icon_Structure");
                        break;
                    case UnitType.Special:
                        template = Resources.Load<UnitScreenElement>("Objects/Templates/Icon_Special");
                        break;
                    default:
                        template = Resources.Load<UnitScreenElement>("Objects/Templates/Icon_Unit");
                        break;
                }
                
                // Instantiate and initialize the unit icon
                Icon = Instantiate(template);
                Icon.Initialize(_iconSprite, UnitColor);
                InvincibleCamera.AppendElement(Icon);
                Icon.SetSelected(false);
            }
            
            // Construct this unit's health bar
            var healthBar = Resources.Load<UnitScreenElement>("Objects/Templates/UI_HealthBar");

            // Instantiate and initialize the health bar
            HealthBar = Instantiate(healthBar);
            HealthBar.Initialize(_healthBarSize, UnitColor);
            InvincibleCamera.AppendElement(HealthBar);
            HealthBar.SetSelected(false);
            
            
            // Deactivate the indicator object if not null
            if (SelectionIndicator != null)
                SelectionIndicator.enabled = false;
            
            // Initialize the target scanner and supporting objects
            ScanInterval = new WaitForSeconds(Random.Range(ScanIntervalRange.x, ScanIntervalRange.y));
            SqrScanRadius = ScanRadius * ScanRadius;
            
            // Calculate the scan layers based on the unit team
            CalculateLayers();
            
            // Start the scanning routine if necessary
            if (AutoAcquireTargets)
                StartCoroutine(nameof(ScanForTargets));
            
            // Set current health = health
            CurrentHealth = Health;
            
            // Call base method
            base.OnRegister();
        }
        
        // Sim Update Callback
        public override void OnSimUpdate(float fixedDelta, bool isHost) {
            // Exit if this object is dying
            if (Dying) return;
            
            // Check for lethal damage
            if (CurrentHealth <= 0f)
                OnDeath();
            
            // Call the base method
            base.OnSimUpdate(fixedDelta, isHost);
        }

        // Render Update Callback
        public override void OnRenderUpdate(float deltaTime) {
            // Exit if this object is dying
            if (Dying) return;
            
            // Update icon screen position
            if (Icon != null) {
                Icon.SetScreenPosition(InvincibleCamera.GetScreenPosition(transform.position));
                Icon.SetRender(InvincibleCamera.IconsRendered);
            }

            // Update healthbar screen position and fill
            if (HealthBar != null) {
                HealthBar.SetScreenPosition(InvincibleCamera.GetScreenPosition(transform.position) + (Vector2.up * 32f));
                HealthBar.SetFill(CurrentHealth / Health);
                HealthBar.SetRender(!InvincibleCamera.IconsRendered);
            }

            // Determine if this object is on screen or not
            if (GeometryUtility.TestPlanesAABB(InvincibleCamera.FrustrumPlanes, UnitRenderer.bounds)) {
                InvincibleCamera.VisibleObjects.Add(this);
                //Icon?.SetRender(true);
                //HealthBar?.SetRender(true);
            }
            else {
                InvincibleCamera.VisibleObjects.Remove(this);
                Icon?.SetRender(false);
                HealthBar?.SetRender(false);
            }
        }
        
        // Set this unit's team and update related objects
        public virtual void SetTeam(Team team) {
            // Exit if this object is dying
            if (Dying) return;
            
            // Set new teama nd related values
            UnitTeam = team;
            UnitColor = TeamColor.GetTeamColor(team);
            Icon?.SetColor(UnitColor);
            
            // Recalculate layers and update the scanner
            CalculateLayers();
        }
        
        // Calculate the layers and masks for this unit
        protected virtual void CalculateLayers() {
            // Make sure this unit's layer is set to match it's team
            gameObject.layer = TeamLayerBounds[0] + (int) UnitTeam;
            
            // Reset the scan mask to be safe
            ScanLayers = new LayerMask();
            
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
                    CurrentTarget = TargetScanner.ScanForNearestTarget(transform.position, ScanRadius, ScanLayers);
					
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

                    if (!WaitingForTarget) 
                        WaitingForTarget = true;
                }
				
                yield return ScanInterval;
            }
        }
        
        // Called when this unit is selected
        public virtual void OnSelected() {
            // Exit if this object is dying
            if (Dying) return;
            
            // Show selection indicator if icons are not rendered
            SelectionIndicator.enabled = true;
            
            // Set icon state to selected
            Icon.SetSelected(true);
            
            // Set selected flag
            Selected = true;
        }
        
        // Called when this unit is deselected
        public virtual void OnDeselected() {
            // Exit if this object is dying
            if (Dying) return;
            
            // Hide selection indicator
            if (SelectionIndicator != null)
                SelectionIndicator.enabled = false;
            
            // Set icon state to unselected
            Icon?.SetSelected(false);
            
            // Set selected flag
            Selected = false;
        }
        
        // Processes a given command
        public virtual void ProcessCommand(UnitCommand command) {
            // Exit if this object is dying
            if (Dying) return;
            
            // Relay the command to the parser
            CommandParser.ProcessCommand(command);
        }
        
        // Applies the specified damage value to this unit
        public virtual void ApplyDamage(float damage) {
            // Exit if this object is dying
            if (Dying) return;
            
            // Exit if this unit is marked as invulnerable
            if (Invulnerable) return;
            
            // Subtract the damage value from this unit's health
            CurrentHealth -= damage;
        }
        
        // Called when this unit is dealt lethal damage
        public virtual void OnDeath() {
            // Exit if this object is already dying
            if (Dying) return;
            
            // Set the IsDying flag to true
            Dying = true;
            
            // Instantiate the death particle effect
            if (DeathEffect != null) {
                var deathEffect = Instantiate(DeathEffect, transform.position, Quaternion.identity);
                deathEffect.Play();
            }
            
            // Destroy this unit's icon object if necessary
            if (Icon != null)
                Destroy(Icon.gameObject);
            
            // Destroy this unit's health bar object if necessary
            if (HealthBar != null)
                Destroy(HealthBar.gameObject);
            
            // Ensure this unit is removed from the visible objects set
            InvincibleCamera.VisibleObjects.Remove(this);
            
            // Destroy this object
            Destroy(gameObject);
        }
    }
}