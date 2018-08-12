using System;
using System.Collections.Generic;
using InvincibleEngine.CameraSystem;
using InvincibleEngine.Managers;
using InvincibleEngine.SelectionSystem;
using InvincibleEngine.UnitFramework.DataTypes;
using InvincibleEngine.UnitFramework.Enums;
using InvincibleEngine.UnitFramework.Interfaces;
using InvincibleEngine.UnitFramework.Utility;
using InvincibleEngine.WeaponSystem;
using VektorLibrary.EntityFramework.Components;
using UnityEngine;
using VektorLibrary.Utility;
using VektorLibrary.EntityFramework.Singletons;
using Random = UnityEngine.Random;

namespace InvincibleEngine.UnitFramework.Components {
    /// <summary>
    /// Base class for all unit behaviors.
    /// Base methods for this class should be called first in any overrides.
    /// </summary>
    [RequireComponent(typeof(GlowingObject))]
    public class UnitBehavior : EntityBehavior, IUnit {
        // Constant: Team Layers Start/End
        public static readonly int[] TeamLayerBounds = {11, 18};
        
        // Time Slicing Stuff
        protected static readonly int TimeSlicingWindow = (int)(1f / EntityManager.FIXED_TIMESTEP);
        protected static int[] IntervalBuffer = new int[TimeSlicingWindow];
        protected static Stack<int> SliceIntervals = new Stack<int>();

        // Unity Inspector        
        [Header("General Settings")]
        [SerializeField] private UnitType _unitType;
        [SerializeField] protected float Health = 100f;
        [SerializeField] protected int _cost = 100;
       
        [Header("Gameplay UI Elements")]
        [SerializeField] private Sprite _iconSprite;
        [SerializeField] private Sprite _healthSprite;

        [Header("Energy Shield")] 
        [SerializeField] protected EnergyShield ShieldPrefab;
        [SerializeField] protected Transform ShieldAnchor;
        [SerializeField] protected bool CalculateShieldRadius = true;
        [SerializeField] protected float ShieldRadius;
        [SerializeField] protected float ShieldHealth = 100f;
        [SerializeField] protected float ShieldRechargeRate = 20f;
        [SerializeField] protected float ShieldRechargeDelay = 5f;

        [Header("Weapons & Combat")] 
        [SerializeField] protected WeaponBehavior WeaponPrefab;
        [SerializeField] protected Transform WeaponAnchor;
        [SerializeField] protected bool AutoAcquireTargets;
        [SerializeField] protected TargetingMode TargetingMode = TargetingMode.Random;
        [SerializeField] protected float ScanRadius = 50f;

        [Header("Destruction Aesthetics")] 
        [SerializeField] protected ParticleSystem DeathEffect;

        //Construction
        [SerializeField] public BuildOption[] ConstructionOptions;
        [SerializeField] public UnitBehavior[] BuildOptions;
        
        // Protected: Time Slicing
        protected int SliceIndex;

        // Protected: Unit Icon
        protected UnitScreenSprite Icon;
        protected UnitScreenSprite HealthBar;
        
        // Protected: Selection Indicator
        protected GlowingObject SelectionIndicator;
        
        // Protected: Component References
        protected MeshRenderer UnitRenderer;
        
        // Protected: Command Processing
        protected CommandParser CommandParser = new CommandParser();
        
        // Protected: Attached Objects
        protected EnergyShield ShieldReference;
        protected WeaponBehavior WeaponReference;
        
        // Protected: Weapons & Combat
        protected WaitForSeconds ScanInterval;
        protected bool WaitingForTarget = true;
        protected float SqrScanRadius;
        
        // Properties: Unit Metadata
        public UnitType UnitType => _unitType;
        public ETeam UnitTeam { get; protected set; }
        public Color UnitColor { get; protected set; }
        public float CurrentHealth { get; protected set; }
        public Sprite IconSprite => _iconSprite;
        public UnitCommands SupportedCommands { get; protected set; }
        public int Cost => _cost;
        
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
        
        // Initialization
        public override void OnRegister() {
            // Calculate time slicing index
            GetTimeSlice();
            
            // Reference required components
            UnitRenderer = GetComponentInChildren<MeshRenderer>();
            SelectionIndicator = GetComponentInChildren<GlowingObject>();
            
            // Fetch team color from team
            UnitColor = TeamColor.GetTeamColor(UnitTeam);
            
            // Set instanced material properties
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_TeamColor", UnitColor);
            properties.SetColor("_EmissionColor", UnitColor);
            UnitRenderer.SetPropertyBlock(properties);
            
            // Construct this unit's icon if possible
            if (_iconSprite != null) {            
                // Load the appropriate template for the unit type
                var template = AssetManager.LoadAsset<UnitScreenSprite>("Objects/Templates/UnitScreenSprite");
                
                // Instantiate and initialize the unit icon
                Icon = Instantiate(template);
                Icon.Initialize(_iconSprite, UnitColor);
                ScreenSpriteManager.AppendSprite(Icon);
                Icon.SetSelected(false);
            }
            
            // Construct this unit's health bar
            if (_healthSprite != null) {
                var healthBar = AssetManager.LoadAsset<UnitScreenSprite>("Objects/Templates/UnitScreenSprite");

                // Instantiate and initialize the health bar
                HealthBar = Instantiate(healthBar);
                HealthBar.Initialize(_healthSprite, UnitColor);
                ScreenSpriteManager.AppendSprite(HealthBar);
                HealthBar.SetSelected(false);
            }
            
            // Initialize the target scanner and supporting objects
            SqrScanRadius = ScanRadius * ScanRadius;
            
            // Calculate the scan layers based on the unit team
            CalculateLayers();
            
            // Spawn in and initialize the energy shield prefab if set
            if (ShieldPrefab != null) {
                // Set shield anchor to this transform if null
                if (ShieldAnchor == null) ShieldAnchor = transform;
                
                // Calculate shield radius if necessary
                var radius = CalculateShieldRadius ? Vector3.Distance(UnitRenderer.bounds.center, UnitRenderer.bounds.max) : ShieldRadius;
                
                // Instantiate and initialize the energy shield
                ShieldReference = Instantiate(ShieldPrefab, ShieldAnchor.position, Quaternion.identity);
                ShieldReference.Initialize(ShieldRadius, ShieldHealth, ShieldRechargeRate, ShieldRechargeDelay, gameObject.layer);
            }
            
            // Spawn in and initialize the weapon prefab if set
            if (WeaponPrefab != null) {
                // Check for improper config
                if (WeaponAnchor == null) {
                    Debug.LogWarning("The weapon anchor on this unit has not been set!\n" +
                                     "Will use object origin instead.");
                    WeaponAnchor = transform;
                }
                
                // Instantiate and initialize the weapon
                WeaponReference = Instantiate(WeaponPrefab, WeaponAnchor.position, Quaternion.identity);
                WeaponReference.Initialize(this);
            }
            
            // Set current health = health
            CurrentHealth = Health;
            
            // Call base method
            base.OnRegister();
        }
        
        // Sim Update Callback
        public override void OnSimUpdate(float fixedDelta, bool isHost) {
            // Run interval update as needed
            if (EntityManager.SimTickCount % TimeSlicingWindow == SliceIndex)
                OnIntervalUpdate();
            
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
            
            // Update shield position if necessary
            if (ShieldReference != null) {
                ShieldReference.transform.position = ShieldAnchor.position;
            }
            
            // Update weapon position if necessary
            if (WeaponReference != null)
                WeaponReference.transform.position = WeaponAnchor.transform.position;
            
            // Update icon screen position
            if (Icon != null) {
                Icon.SetScreenPosition(InvincibleCamera.GetScreenPosition(transform.position));
                Icon.SetRender(InvincibleCamera.IconsRendered);
            }

            // Update healthbar screen position and fill
            if (HealthBar != null) {
                HealthBar.SetScreenPosition(InvincibleCamera.GetScreenPosition(transform.position) + (Vector2.down * 32f));
                HealthBar.SetScale(new Vector2(CurrentHealth / Health, 1f));
                HealthBar.SetRender(InvincibleCamera.HealthBarsRendered);
            }

            // Determine if this object is on screen or not
            if (GeometryUtility.TestPlanesAABB(InvincibleCamera.FrustrumPlanes, UnitRenderer.bounds)) {
                InvincibleCamera.VisibleObjects.Add(this);
            }
            else {
                InvincibleCamera.VisibleObjects.Remove(this);
                Icon?.SetRender(false);
                HealthBar?.SetRender(false);
            }
        }
        
        // Time Slicing Update
        protected virtual void OnIntervalUpdate() {
            // Exit if we don't want automatic scanning
            if (AutoAcquireTargets)
                ScanForTargets(); 
        }
        
        // Target scanning routine
        protected virtual void ScanForTargets() {
            // If we are waiting for a target, initiate a scan
            if (WaitingForTarget) {
                // Scan for targets in range
                CurrentTarget = ObjectScanner.ScanForObject<UnitBehavior>(transform.position, ScanRadius, ScanLayers, TargetingMode);
					
                // Set waiting flag to true if target found
                WaitingForTarget = CurrentTarget == null;
                
                // We're done here
                return;
            }

            // Make sure the current target is alive
            if (CurrentTarget != null) {
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
                if (!WaitingForTarget) 
                    WaitingForTarget = true;
            }
        }
        
        // Calculate and set this unit's time slice index
        protected virtual void GetTimeSlice() {
            // Populate and shuffle the stack of slices if it's empty
            if (SliceIntervals.Count == 0) {
                // Generate an array of intervals
                for (var i = 0; i < TimeSlicingWindow; i++)
                    IntervalBuffer[i] = i;
                
                // Shuffle the intervals
                IntervalBuffer.Shuffle();
                
                // Add the shuffled intervals to the stack
                for (var i = 0; i < IntervalBuffer.Length; i++) {
                    SliceIntervals.Push(IntervalBuffer[i]);
                }
            }
            
            // Grab an interval from the stack
            SliceIndex = SliceIntervals.Pop();
        }
        
        // Set this unit's team and update related objects
        public virtual void SetTeam(ETeam team) {
            // Exit if this object is dying
            if (Dying) return;
            
            // Set new team and related values
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
            foreach (var team in Enum.GetValues(typeof(ETeam))) {
                // Skip this unit's team
                if ((ETeam) team == UnitTeam) continue;
                
                // Set the appropriate bit flags
                ScanLayers = ScanLayers | (int)Mathf.Pow(2, TeamLayerBounds[0] + (int) team);
            }
        }
        
        // Called when this unit is selected
        public virtual void OnSelected() {
            // Exit if this object is dying
            if (Dying) return;
            
            // Set icon state to selected
            Icon?.SetSelected(true);
            SelectionIndicator?.SetTargetColor(UnitColor);
            
            // Set selected flag
            Selected = true;
        }
        
        // Called when this unit is deselected
        public virtual void OnDeselected() {
            // Exit if this object is dying
            if (Dying) return;
            
            // Set icon state to unselected
            Icon?.SetSelected(false);
            SelectionIndicator?.SetTargetColor(Color.black);
            
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
                var deathEffect = ObjectManager.GetObject(DeathEffect.gameObject, transform.position, Quaternion.identity);
            }
            
            // Destroy this unit's shield if necessary
            if (ShieldReference != null)
                Destroy(ShieldReference.gameObject);
            
            // Destroy this unit's weapon if necessary
            if (WeaponReference != null)
                Destroy(WeaponReference.gameObject);
            
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