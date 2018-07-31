using InvincibleEngine;
using System.Collections;
using System.Collections.Generic;
using InvincibleEngine.UnitFramework.Components;
using UnityEngine;
using SteamNet;

public class StructureBehavior : UnitBehavior {

    //Structure Properties
    [Header("Structure Properties")]
    [SerializeField] private float BuildTime = 5;
    [SerializeField] private float BuildResources = 0;
    [SerializeField] private Vector3 Origin;
    [SerializeField] private float Height;
    [SerializeField] private GameObject BuildPoint;   

    [Header("Resource Generation")]
    [SerializeField] private int EnergyGen = 0;
    [SerializeField] private int ResourceGen = 0;

    //Static effects
    public ParticleSystem BuildingEffect;

    //OnStart
    public override void Start() {
        base.Start();


    }

    //Sim Update
    public override void OnSimUpdate(float fixedDelta, bool isHost) { 
        base.OnSimUpdate(fixedDelta, isHost);



    }

    //Econ Update
    public override void OnEconomyUpdate(float fixedDelta, bool isHost) {
        base.OnEconomyUpdate(fixedDelta, isHost);

        //Do no economy if not a host
        if (!isHost) { return; }


        //Generate resources according to generation
        SteamNetManager.Instance.CurrentlyJoinedLobby.LobbyMembers[PlayerOwner].Economy.OnGenerateResouces(ResourceGen);

    }

}
