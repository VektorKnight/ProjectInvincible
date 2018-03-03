using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.Components.Utility {
	public class FancyBar : MonoBehaviour {

		public float _backFillSpeed = 20.0f;
		public Image _backFill;
		private Image _mainFill;

		// Use this for initialization
		void Start () {
			_mainFill = GetComponent<Image>();
		}
	
		// Update is called once per frame
		void Update () {
			_backFill.fillAmount = Mathf.MoveTowards(_backFill.fillAmount, _mainFill.fillAmount, Mathf.Abs(_mainFill.fillAmount - _backFill.fillAmount) * Time.deltaTime * _backFillSpeed);
			var color = _mainFill.color;
			color.a = 0.5f;
			_backFill.color = color;
		}
	}
}
