﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Target
{
    public GameObject reference;
    public float dist;
}

abstract public class BaseEnemy : Entity {

    protected NavMeshAgent nav_Agent;

    public enum State
    {
        Idle,
        Hunt,
        Patrol,
        Combat
    }

    protected State CurrentState;

    // Combat Variables
    [Header("Combat Variables")]
    public Target[] targets = new Target[2];
    public float huntDistance;
    public float combatDistance;

    protected bool BL_startAsCombat = false;
    public static bool BL_allCombat = false;
    protected Vector3 V_Target;
    protected Vector3 V_Home;
    protected GameObject GO_Target;

    // Weapon variables
    [Header("Weapon Variables")]
    public GameObject go_projectilePrefab;
    public float fl_shotCooldown;
    private float FL_Cooldown;

    private float FL_lerpRate = 10f;

    // Use this for initialization
    virtual public void Start ()
    {
        V_Home = transform.position;
        CurrentState = State.Idle;
        nav_Agent = GetComponent<NavMeshAgent>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (GameManager.instance.GO_Player[0] == null && GameManager.instance.GO_Player[1] == null)
            return;

        AcquireTargets();
        DecideState();
    }

    //Get a list of targets
    void AcquireTargets()
    {
        //Search all players for their distance
        for (int i = 0; i < GameManager.instance.GO_Player.Length; i++)
        {
            if (GameManager.instance.GO_Player[i] == null)
            {
                targets[i].dist = Mathf.Infinity;
            }
            else
            {
                targets[i].reference = GameManager.instance.GO_Player[i];
                targets[i].dist = Vector3.Distance(transform.position, GameManager.instance.GO_Player[i].transform.position);
            }
        }

        //After grabbing the distance of all our players
        //If the distance of player 1 is lower than player 2, make him the target
        if (targets[0].dist < targets[1].dist)
        {
            //Set target Vector to the player's position
            //But know who the player is
            V_Target = GameManager.instance.GO_Player[0].transform.position;
            GO_Target = GameManager.instance.GO_Player[0];
            //Debug.Log("P1 Target");
        }
        //If the distance of player 2 is lower than player 1, make him the target
        else if (targets[1].dist < targets[0].dist)
        {
            V_Target = GameManager.instance.GO_Player[1].transform.position;
            GO_Target = GameManager.instance.GO_Player[1];
            //Debug.Log("P2 Target");
        }
        //If either the two players are the same distance, prioritise player 1 (unlikely to happen)
        else if (targets[0].dist == targets[1].dist)
        {
            V_Target = GameManager.instance.GO_Player[0].transform.position;
            GO_Target = GameManager.instance.GO_Player[0];
            //Debug.Log("Hunt: Values are equal");
        }
    }

    //Decide on a state
    void DecideState()
    {
        //State switching

        if (CurrentState == State.Idle && !BL_startAsCombat)
            Idle();
        else if (CurrentState == State.Hunt)
            Hunt();
        else if (CurrentState == State.Combat)
            Combat();
    }

    //--------------------------------------------------------
    //------ States
    //--------------------------------------------------------

    //Idle Mode
    virtual public void Idle()
    {
        //Either move between waypoints or stay still
        //---
        if (Vector3.Distance(transform.position, V_Target) < huntDistance) CurrentState = State.Hunt;
    }

    //Hunt Mode
    virtual public void Hunt()
    {
        nav_Agent.destination = V_Target;

        //---
        if (Vector3.Distance(transform.position, V_Target) < combatDistance) CurrentState = State.Combat;
        else if (Vector3.Distance(transform.position, V_Target) > huntDistance + 5 && !BL_startAsCombat && !BL_allCombat)
        {
            nav_Agent.destination = V_Home;
            CurrentState = State.Idle;
        }
    }

    virtual public void Combat()
    {
        //SpawnBullet
        BL_allCombat = true;
        transform.LookAt(V_Target);
        if (Vector3.Distance(transform.position, V_Target) > 10.0f)
        {
            nav_Agent.isStopped = false;
            nav_Agent.destination = V_Target;
        }
        else nav_Agent.isStopped = true;


        if (Time.time > FL_Cooldown)
        {
            FireBullet();
            FL_Cooldown = Time.time + fl_shotCooldown;
        }

        //---
        if (Vector3.Distance(transform.position, V_Target) > combatDistance + 5)
        {
            CurrentState = State.Hunt;
            V_Home = transform.position;
        }
    }

    virtual public void FireBullet()
    {
        // Create a bullet and reset the shot timer

        var _bullet = (GameObject)Instantiate(go_projectilePrefab, transform.position + transform.TransformDirection(new Vector3(0, 1, 1.5F)), transform.rotation);

    }
}