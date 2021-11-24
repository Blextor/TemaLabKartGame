using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Samples;
using UnityEngine;

public class HelloWorldPlayer : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<Vector3> Velocity = new NetworkVariable<Vector3>();
    public NetworkVariable<float> GlobalSpeedModifyer = new NetworkVariable<float>();
    public NetworkVariable<bool> BtnW = new NetworkVariable<bool>(); // W A S D Space Shift 0-5
    public NetworkVariable<bool> BtnA = new NetworkVariable<bool>();
    public NetworkVariable<bool> BtnS = new NetworkVariable<bool>();
    public NetworkVariable<bool> BtnD = new NetworkVariable<bool>();
    public NetworkVariable<bool> BtnSpace = new NetworkVariable<bool>();
    public NetworkVariable<bool> BtnShift = new NetworkVariable<bool>();
    public bool[] WASDSS = new bool[6];

    public Rigidbody rigi;
    public NetworkTransform2 networkTransform;
    

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Move();

            
        }
    }

    public void Move()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
            GlobalSpeedModifyer.Value = 10f;
        }
        else
        {
            SubmitPositionRequestServerRpc();
        }
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Position.Value = GetRandomPositionOnPlane();
        transform.position = Position.Value;
        GlobalSpeedModifyer.Value = 10f;
    }


    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }



    [ServerRpc]
    void WBtnDownServerRpc(ServerRpcParams rpcParams = default)
    {
        BtnW.Value = true;
        WASDSS[0] = true;
    }

    [ServerRpc]
    void WBtnUpServerRpc(ServerRpcParams rpcParams = default)
    {
        BtnW.Value = false;
        WASDSS[0] = false;
    }

    [ServerRpc]
    void SBtnDownServerRpc(ServerRpcParams rpcParams = default)
    {
        BtnS.Value = true;
        WASDSS[2] = true;
    }

    [ServerRpc]
    void SBtnUpServerRpc(ServerRpcParams rpcParams = default)
    {
        BtnS.Value = false;
        WASDSS[2] = false;
    }

    [ServerRpc]
    void ABtnDownServerRpc(ServerRpcParams rpcParams = default)
    {
        BtnA.Value = true;
        WASDSS[1] = true;
    }

    [ServerRpc]
    void ABtnUpServerRpc(ServerRpcParams rpcParams = default)
    {
        BtnA.Value = false;
        WASDSS[1] = false;
    }

    [ServerRpc]
    void DBtnDownServerRpc(ServerRpcParams rpcParams = default)
    {
        BtnS.Value = true;
        WASDSS[3] = true;
    }

    [ServerRpc]
    void DBtnUpServerRpc(ServerRpcParams rpcParams = default)
    {
        BtnS.Value = false;
        WASDSS[3] = false;
    }

    [ServerRpc]
    void MoveRlyRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Vector3 velo = MoveRly();
        Velocity.Value = velo;
        rigi.AddForceAtPosition(velo, transform.position);
    }

    Vector3 MoveRly()
    {
        Vector3 ret = new Vector3(0, 0, 0);
        if (WASDSS[0])
        {
            ret.z += GlobalSpeedModifyer.Value;
        }
        if (WASDSS[2])
        {
            ret.z -= GlobalSpeedModifyer.Value;
        }
        if (WASDSS[1])
        {
            ret.x -= GlobalSpeedModifyer.Value;
        }
        if (WASDSS[3])
        {
            ret.x += GlobalSpeedModifyer.Value;
        }
        return ret;
    }

    void FixedUpdate()
    {
        
        if (IsOwner)
        {
            //networkTransform.Interpolate = !networkTransform.Interpolate;
            //networkTransform.
            Debug.Log("OK");

            if (NetworkManager.Singleton.IsServer)
            {
                BtnW.Value = Input.GetKey(KeyCode.W);
                BtnS.Value = Input.GetKey(KeyCode.S);
                BtnA.Value = Input.GetKey(KeyCode.A);
                BtnD.Value = Input.GetKey(KeyCode.D);
                BtnShift.Value = Input.GetKey(KeyCode.LeftShift);
                BtnSpace.Value = Input.GetKey(KeyCode.Space);

                WASDSS[0] = Input.GetKey(KeyCode.W);
                WASDSS[2] = Input.GetKey(KeyCode.S);
                WASDSS[1] = Input.GetKey(KeyCode.A);
                WASDSS[3] = Input.GetKey(KeyCode.D);
                WASDSS[4] = Input.GetKey(KeyCode.LeftShift);
                WASDSS[5] = Input.GetKey(KeyCode.Space);
            }
            else
            {
                if (Input.GetKey(KeyCode.W))
                    WBtnDownServerRpc();
                else
                    WBtnUpServerRpc();
                if (Input.GetKey(KeyCode.S))
                    SBtnDownServerRpc();
                else
                    SBtnUpServerRpc();
                if (Input.GetKey(KeyCode.A))
                    ABtnDownServerRpc();
                else
                    ABtnUpServerRpc();
                if (Input.GetKey(KeyCode.D))
                    DBtnDownServerRpc();
                else
                    DBtnUpServerRpc();
            }

            if (NetworkManager.Singleton.IsServer)
            {
                Vector3 velo = MoveRly();
                rigi.AddForceAtPosition(velo, transform.position);
            }
            else
            {
                MoveRlyRequestServerRpc();
                rigi.AddForceAtPosition(Velocity.Value,transform.position);
            }
        }
        
    }
}
