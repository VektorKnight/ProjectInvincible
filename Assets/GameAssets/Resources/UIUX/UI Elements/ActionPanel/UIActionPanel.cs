using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SteamNet;
using InvincibleEngine;
using InvincibleEngine.Managers;

public class UIActionPanel : MonoBehaviour {

    //Object displaying actions of
    private EntityBehavior _CurrentlySelectedObject;

    //Prefab for displaying actions
    public UIAction ActionPrefab;

    void Update() {

        //if nothing is selected, clear build list
        if (PlayerManager.Instance.SelectedEntities.Count == 0) {

            //clear current actions
            foreach (Transform child in transform) {
                GameObject.Destroy(child.gameObject);
            }

            //reset currently selected object
            _CurrentlySelectedObject = null;

            //break out of loop, nothing selected
            return;
        }

        //Check to see if the selected unit changed
        if (PlayerManager.Instance.SelectedEntities.Count > 0 && (_CurrentlySelectedObject == null || PlayerManager.Instance.SelectedEntities[0].name != _CurrentlySelectedObject.name)) {
            
            //Set currently actioned entity
            _CurrentlySelectedObject = PlayerManager.Instance.SelectedEntities[0];

            //clear current actions
            foreach (Transform child in transform) {
                GameObject.Destroy(child.gameObject);
            }

            //generate actions for object
            foreach (BuildOption n in _CurrentlySelectedObject.BuildOptions) {

                //instantiate object
                UIAction u = Instantiate(ActionPrefab, transform);

                //Set values for object
                u.Action = n.PrefabBuild;
                u.DisplayImage.sprite = n.PrefabBuild.Icon;
            }
        }
    }
}
