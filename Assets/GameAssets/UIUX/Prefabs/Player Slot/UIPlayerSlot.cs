using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using SteamNet;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;

public class UIPlayerSlot : MonoBehaviour {
    [SerializeField]
    private Text _Name;

    public CSteamID CardID;
    public string Name {
        get { return _Name.text; }
        set { _Name.text = value; }
    }


}
