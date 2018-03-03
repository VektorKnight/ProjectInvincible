using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.UI_Elements {
    public class UI_Parallax : MonoBehaviour {
        public enum ParallaxType {Material, Positon}
        private Vector3 ScreenSize { get { return new Vector3(Screen.width, Screen.height,0); } }
        private Vector2 MousePosition { get { return Input.mousePosition-(ScreenSize/2); } }
        private Image Rend;
        [SerializeField]
        private float sensitivity = 10;
        public ParallaxType parallaxType = ParallaxType.Material;
        private Vector3 startingPosition;
        void Start () {
            Rend = GetComponent<Image>();
            startingPosition = transform.position;
        }

        // Update is called once per frame
        void Update() {
            if (parallaxType == ParallaxType.Material) {
                Rend.material.SetTextureOffset("_MainTex", Vector2.Lerp(Rend.material.GetTextureOffset("_MainTex"), Input.mousePosition / (sensitivity),0.01f));
            }
            if(parallaxType== ParallaxType.Positon) {
                transform.position = Vector3.Lerp(transform.position, new Vector3(startingPosition.x + (MousePosition.x / sensitivity), startingPosition.y + (MousePosition.y / sensitivity),startingPosition.z),0.01f);
            }
        }
    }
}
