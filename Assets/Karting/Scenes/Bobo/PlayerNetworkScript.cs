/*
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if MULTIPLAYER_TOOLS
using Unity.Multiplayer.Tools;
#endif
using Unity.Profiling;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;



class HelloWorldPlayer_DataForServer
{
    public Vector3 position;

    public HelloWorldPlayer_DataForServer(Vector3 pos)
    {
        position = pos;
    }
}
public class PlayerNetworkScript : NetworkBehaviour
{
    public float MovementSpeed;
    public float framerate;
    private HelloWorldPlayer_DataForServer data;

    public float speed;
    public Vector3 poswas;
    public float lastTime;
    public float duration;

    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariableBool[] moveset = new NetworkVariableBool[6]; // W A S D Space Shift 0-5

    public override void NetworkStart()
    {
        Move();
        for (int i = 0; i < 6; i++)
        {
            moveset[i] = new NetworkVariableBool(new NetworkVariableSettings
            {
                WritePermission = NetworkVariablePermission.ServerOnly,
                ReadPermission = NetworkVariablePermission.Everyone
            }); ;
        }
    }

    public void Start()
    {
        data = new HelloWorldPlayer_DataForServer(this.transform.position);
        Position.Value = this.transform.position;
    }

    public void Move()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }
        else
        {
            SubmitPositionRequestServerRpc();
        }
    }

    private Vector3 MoveTo(bool W, bool A, bool S, bool D)
    {
        Vector3 newpos = Position.Value;
        if (W)
            newpos.z += MovementSpeed * Time.deltaTime;
        if (A)
            newpos.x -= MovementSpeed * Time.deltaTime;
        if (S)
            newpos.z -= MovementSpeed * Time.deltaTime;
        if (D)
            newpos.x += MovementSpeed * Time.deltaTime;

        return newpos;
    }
    public void Move2(bool W, bool A, bool S, bool D)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Vector3 pos = MoveTo(W, A, S, D);
            transform.position = pos;
            Position.Value = pos;
            data.position = Position.Value;
        }
        else
        {
            SubmitMoveToRequestServerRpc(W, A, S, D);
        }
        transform.position = Position.Value;


    }

    [ServerRpc]
    void SubmitMoveToRequestServerRpc(bool W, bool A, bool S, bool D)
    {
        Position.Value = MoveTo(W, A, S, D);
        data.position = Position.Value;
        transform.position = data.position;
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Position.Value = GetRandomPositionOnPlane();
        data.position = Position.Value;
        transform.position = data.position;
    }

    [ServerRpc]
    void GetResetServerRpc(ServerRpcParams rpcParams = default)
    {
        Position.Value = new Vector3(0, 2, 0);
        transform.position = new Vector3(0, 2, 0);
        data.position = new Vector3(0, 2, 0);
    }

    private Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 0.5f, Random.Range(-3f, 3f));
    }

    public void CheatResett()
    {
        Position.Value = new Vector3(0, 2, 0);
        transform.position = new Vector3(0, 2, 0);
        data.position = new Vector3(0, 2, 0);
    }

    public void CheatResett2()
    {
        Position.Value = new Vector3(0, 2, 0);
        transform.position = new Vector3(0, 2, 0);
    }

    public void Resett()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Position.Value = new Vector3(0, 2, 0);
            transform.position = new Vector3(0, 2, 0);
            data.position = new Vector3(0, 2, 0);
        }
        else
        {
            GetResetServerRpc();
        }
        transform.position = Position.Value;
    }

    [ServerRpc]
    void GetRefreshServerRpc(ServerRpcParams rpcParams = default)
    {
        Position.Value = data.position;
        transform.position = data.position;

    }

    public void Refresh()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Position.Value = data.position;
            transform.position = data.position;
        }
        else
            GetRefreshServerRpc();
        transform.position = Position.Value;
    }

    private void TryMove()
    {

        bool W, A, S, D;


        W = moveset[0].Value;
        A = moveset[1].Value;
        S = moveset[2].Value;
        D = moveset[3].Value;

        if (W || A || S || D)
            Move2(W, A, S, D);

    }

    [ServerRpc]
    void GetInputsServerRpc(bool[] keyset)
    {
        for (int i = 0; i < 6; i++)
            moveset[i].Value = keyset[i];
    }
    public void GetInputs(bool[] keyset)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            for (int i = 0; i < 6; i++)
                moveset[i].Value = keyset[i];
        }
        else
        {
            GetInputsServerRpc(keyset);
        }
    }

    bool isMoveing()
    {
        return (moveset[0].Value || moveset[1].Value || moveset[2].Value || moveset[3].Value || moveset[4].Value || moveset[5].Value);
    }

    void FixedUpdate()
    {
        if (isMoveing())
        {
            if (NetworkManager.Singleton.IsServer)
            {
                TryMove();
            }
        }
    }
    void Update()
    {
        if (lastTime + duration < Time.realtimeSinceStartup)
        {
            speed = (Position.Value - poswas).magnitude / duration;
            poswas = Position.Value;
            lastTime = Time.realtimeSinceStartup;
        }
        //transform.position = Position.Value;
        transform.position = Vector3.Lerp(transform.position, Position.Value, 0.4f);
        //framerate=GameObject.FindGameObjectWithTag("FPSCalc").GetComponent<FPS_Script>().FPS;
    }
}
*/