using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InvincibleEngine;
using InvincibleEngine.Managers;
using VektorLibrary.EntityFramework.Components;

public class UIAction : MonoBehaviour {
    public Image DisplayImage;
    public EntityBehavior Action;

    public void OnAction() {
        PlayerManager.Instance.OnBuildRequest(Action);
    }
}
