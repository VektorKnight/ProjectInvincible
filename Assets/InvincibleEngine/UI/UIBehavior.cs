using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Inherit to get UI events for displaying things
/// </summary>
public class UIBehavior : MonoBehaviour {


    //------------------------------------
    #region virtual methods
    //------------------------------------
    
    public virtual void OnMatchStart() {

    }


    #endregion



    //------------------------------------
    #region Subscribe overrides
    //------------------------------------

    //Subscribe 
    private void OnEnable() {
        MatchManager.OnMatchStartEvent += OnMatchStart;
    }

    //Unsubscribe
    private void OnDisable() {
        MatchManager.OnMatchStartEvent -= OnMatchStart;

    }

    #endregion

}
