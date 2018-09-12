using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SteamNet;
using InvincibleEngine;
using InvincibleEngine.Managers;
using VektorLibrary.EntityFramework.Components;
using InvincibleEngine.UnitFramework.Components;
using System;
/// <summary>
/// Handles the unit actions panel component of the in-game UI.
/// --Edited by VektorKnight: Refactored to use new selection events from PlayerManager.
/// </summary>
public class UIActionPanel : MonoBehaviour {

    //Object displaying actions of
    private UnitBehavior _CurrentlySelectedObject;

    //Prefab for displaying actions
    public UIAction ActionPrefab;
    
    // Initialization
    private void Start() {
        // Register event handlers with player manager
        PlayerManager.OnUnitsSelected += OnUnitsSelected;
        PlayerManager.OnUnitsDeselected += OnUnitsDeselected;
    }
    
    // OnUnitsSelected event handler
    private void OnUnitsSelected(List<UnitBehavior> units) {
        // Return if the list is empty
        if (units.Count == 0) return;

        _CurrentlySelectedObject = units[0];

        // Generate actions for object
        foreach (var n in _CurrentlySelectedObject.ConstructionOptions) {
            //instantiate object
            var u = Instantiate(ActionPrefab, transform);

            // Set values for object
            u.Action = n.PrefabBuild;
            u.DisplayImage.sprite = n.PrefabBuild.IconSprite;
        }
        
    }
    
    // OnUnitsDeselected event handler
    private void OnUnitsDeselected() {
        // Clear current actions
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        // Reset currently selected object
        _CurrentlySelectedObject = null;
    }
    
    // Destroy callback
    private void OnDestroy() {
        // Unregister event handlers with player manager
        PlayerManager.OnUnitsSelected -= OnUnitsSelected;
        PlayerManager.OnUnitsDeselected -= OnUnitsDeselected;
    }
}
