using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using InvincibleEngine.Managers;
using InvincibleEngine.UnitFramework.Components;
using InvincibleEngine.UnitFramework.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.UI {
    public class UnitFactoryPanel : UIBehavior {
        // Unity Inspector
        [Header("Required UI Elements")] 
        [SerializeField] private HorizontalLayoutGroup _buildableUnitsLayout;
        [SerializeField] private HorizontalLayoutGroup _buildListLayout;

        [Header("Required UI Prefabs")] 
        [SerializeField] private FactoryBuildButton _buildableUnitPrefab;
        [SerializeField] private FactoryOrderButton _buildOrderPrefab;

        // UI Element Collections
        private List<FactoryBuildButton> _buildButtons;
        private List<FactoryOrderButton> _buildOrders;

        // Factory Target
        private FactoryBehavior _targetFactory;

        // State
        private bool _initialized;

        // Initialization
        private void Start() {
            // Ensure all required references are set
            if (!_buildableUnitsLayout || !_buildListLayout || !_buildableUnitPrefab || !_buildOrderPrefab) {
                Debug.LogError("Required references are not set! Cannot initialize UI element.");
                return;
            }

            // Initialize lists
            _buildButtons = new List<FactoryBuildButton>();
            _buildOrders = new List<FactoryOrderButton>();

            // Register with selection events
            PlayerManager.OnUnitsSelected += OnUnitsSelected;
            PlayerManager.OnUnitsDeselected += OnUnitsDeselected;

            // Set initialization flag
            _initialized = true;
        }

        // Selection event handler
        private void OnUnitsSelected(List<UnitBehavior> units) {
            // Exit if not initialized
            if (!_initialized) return;

            // For now we should just link to the first factory in the list
            for (var i = 0; i < units.Count; i++) {
                // Skip non-factory units
                if ((units[i].Features & UnitFeatures.Factory) != UnitFeatures.Factory) return;

                // Try to cast to a factory behavior and link if successful
                var factory = units[i] as FactoryBehavior;

                // Skip if unsuccessful
                if (factory == null) continue;

                // Link to the factory and break out of the loop
                _targetFactory = factory;
                _targetFactory.OnBuildListChanged += OnBuildListChanged;
                break;
            }

            // Populate UI elements
            RepopulateElements();
        }

        // Repopulates the UI elements
        private void RepopulateElements() {
            //Clear all elements
            ClearElements();

            // Populate the buildable unit buttons
            for (var i = 0; i < _targetFactory.BuildableUnits.Count; i++) {
                // Reference the current unit
                var unit = _targetFactory.BuildableUnits[i];

                // Skip if null
                if (unit == null) continue;

                // Instantiate a new button
                var buildButton = Instantiate(_buildableUnitPrefab, _buildableUnitsLayout.transform);

                // Initialize the button
                buildButton.Initialize(null, unit.IconSprite, i, _targetFactory.TryAddOrder);
                _buildButtons.Add(buildButton);
            }

            // Populate the build order buttons
            for (var i = 0; i < _targetFactory.BuildList.Count; i++) {
                // Reference the current unit
                var order = _targetFactory.BuildList[i];

                // Instantiate a new button
                var orderButton = Instantiate(_buildOrderPrefab, _buildListLayout.transform);

                // Initialize the button
                orderButton.Initialize(null, order.Key.IconSprite, order.Value, i, _targetFactory.TryEditOrder, _targetFactory.TryCancelOrder);
                _buildOrders.Add(orderButton);
            }
        }

        // Clears UI elements
        private void ClearElements() {
            // Destroy each build button
            for (var i = 0; i < _buildButtons.Count; i++) {
                Destroy(_buildButtons[i].gameObject);
            }

            for (var i = 0; i < _buildOrders.Count; i++) {
                Destroy(_buildOrders[i].gameObject);
            }

            _buildButtons.Clear();
            _buildOrders.Clear();
        }


        // Deselection event handler
        private void OnUnitsDeselected() {
            ClearElements();

            // Reset the target factory reference
            if (_targetFactory != null)
                _targetFactory.OnBuildListChanged -= OnBuildListChanged;
            _targetFactory = null;
        }

        private void OnBuildListChanged() {
            RepopulateElements();
        }
    }
}
