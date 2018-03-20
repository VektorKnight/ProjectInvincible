using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UI_Dropdown : MonoBehaviour {
    public int SelectedOption = 0;

    public GameObject Template;
    public GameObject Content;


    [System.Serializable]
    public struct Option {
        public string Label;
        public Sprite Image;
    }

    public List<Option> Options;
   
}
