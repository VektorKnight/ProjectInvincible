using InvincibleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_BuildMenu : MonoBehaviour {
    public GameObject BuildOptionPrefab;
    public void OnBuild() {
        MatchManager.Instance.OnPlayerBuild(MatchManager.Instance.BuildOptions[0]);
    }
}
