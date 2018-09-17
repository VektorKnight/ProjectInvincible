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
    public class FactoryOrderButton : MonoBehaviour {
        // Unity Inspector
        [Header("Required UI Elements")]
        [SerializeField] private Image _preview;
        [SerializeField] private Image _icon;
        [SerializeField] private Text _count;

        // Index & Action
        private int _orderIndex;
        private int _orderCount;
        private Func<int, int, bool> _factoryEdit;
        private Func<int, bool> _factoryCancel;

        // Initializes this object
        public void Initialize(Sprite preview, Sprite icon, int count, int unitIndex, Func<int, int, bool> factoryEdit, Func<int, bool> factoryCancel) {
            _preview.sprite = preview != null ? preview : _preview.sprite;
            _icon.sprite = icon != null ? icon : _icon.sprite;
            _count.text = $"x{count}";
            _orderCount = count;
            _orderIndex = unitIndex;
            _factoryEdit = factoryEdit;
            _factoryCancel = factoryCancel;
        }

        // Button OnClick Handler
        public void OnClicked() {
            if (Input.GetKey(KeyCode.LeftShift) || _orderCount - 1 == 0) {
                _factoryCancel.Invoke(_orderIndex);
            }
            else {
                _factoryEdit.Invoke(_orderIndex, _orderCount - 1);
            }
        }
    }
}
