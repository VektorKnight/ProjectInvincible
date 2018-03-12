using UnityEngine;

namespace InvincibleEngine.Components.Testing {
    public class TestEntity2 : MonoBehaviour {
        private void Update() {
            transform.position += Vector3.up * Mathf.Sin(Time.time);
        }
    }
}