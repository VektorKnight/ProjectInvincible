using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InvincibleEngine.UI {
    /// <summary>
    /// Used for displaying which units a factory can build.
    /// </summary>
    public class FactoryBuildButton : MonoBehaviour {
        // Unity Inspector
        [Header("Required UI Elements")]
        [SerializeField] private Image _preview;
        [SerializeField] private Image _icon;

        // Index & Action
        private int _unitIndex;
        private Func<int, int, bool> _factoryAction;

        // Initializes this object
        public void Initialize(Sprite preview, Sprite icon, int unitIndex, Func<int, int, bool> factoryAction) {
            _preview.sprite = preview != null ? preview : _preview.sprite;
            _icon.sprite = icon != null ? icon : _icon.sprite;
            _unitIndex = unitIndex;
            _factoryAction = factoryAction;
        }

        // Button OnClick Handler
        public void OnClicked() {
            var count = Input.GetKey(KeyCode.LeftShift) ? 10 : 1;
            _factoryAction.Invoke(_unitIndex, count);
        }
    }
}
