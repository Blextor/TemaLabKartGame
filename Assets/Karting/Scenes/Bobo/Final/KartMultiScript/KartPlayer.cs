using PathCreation.Examples;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Samples;
using UnityEngine;

public class KartPlayer : NetworkBehaviour
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
    //public NetworkList<bool> WASDNet;
    public bool[] WASDSS = new bool[6];

    public Rigidbody rigi;
    public NetworkTransform2 networkTransform;
    public GameObject ExtraPointGameObject;
    public GameObject ExtraPointColliderGameObject;

    public float time;
    public float lastPowerUp;

    public NetworkVariable<float> PointsNet = new NetworkVariable<float>();
    public float points;

    public Material[] colors = new Material[4];
    public Material myMaterial;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Move();
            //GetMapServerRpc();

        }
    }

    /*
    [ServerRpc]
    void GetMapServerRpc()
    {
        Debug.Log("Talán itt");
        RoadMeshCreator script = GameObject.FindGameObjectWithTag("RoadCreator").GetComponent<RoadMeshCreator>();
        Vector3[] cpoints = script.CirclePoints;
        Vector3[] rpoints = script.RoadPoints;
        UpdateMapClientRpc(cpoints, rpoints);

        //var player = playerObject.GetComponent<HelloWorldPlayer>();
        //player.Move();
    }
    */

    [ClientRpc]
    public void GetReadyClientRpc(float x, float y, float z, Vector3 forw, ClientRpcParams rpcParams = default)
    {
        this.transform.position = new Vector3(x, y, z);
        this.transform.LookAt(this.transform.position + forw);
        //Position.Value = new Vector3(x, y, z);
        Debug.Log("Player: " + "hapci");
    }

    /*[ServerRpc]
    void AddParamServerRpc(ServerRpcParams rpcParams = default)
    {
        WASDNet.Add(false);
    }*/

    public void Start()
    {
        /*WASDNet = new NetworkList<bool>();
        for (int i=0; i < 4; i++)
        {
            AddParamServerRpc();
        }*/
        //myMaterial = GetComponent<Renderer>().material;
        //GetComponent<Renderer>().material = colors[GameObject.FindGameObjectsWithTag("BoboPlayer").Length-1];
        //myMaterial = colors[GameObject.FindGameObjectsWithTag("BoboPlayer").Length]; 
        //WASDNet.Insert()
    }

    public void Move() 
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
            GlobalSpeedModifyer.Value = 10f;
            //myMaterial = colors[GameObject.FindGameObjectsWithTag("BoboPlayer").Length];
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
        //myMaterial = colors[GameObject.FindGameObjectsWithTag("BoboPlayer").Length];
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
        BtnD.Value = true;
        WASDSS[3] = true;
    }

    [ServerRpc]
    void DBtnUpServerRpc(ServerRpcParams rpcParams = default)
    {
        BtnD.Value = false;
        WASDSS[3] = false;
    }
    

    [ServerRpc]
    void GetInputServerRpc(bool W, bool A, bool S, bool D, ServerRpcParams rpcParams = default)
    {
        WASDSS[0] = W;
        WASDSS[1] = A;
        WASDSS[2] = S;
        WASDSS[3] = D;
        /*WASDNet[0] = W;
        WASDNet[1] = A;
        WASDNet[2] = S;
        WASDNet[3] = D;*/
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

    [ServerRpc]
    private void SpawnExtrapointServerRpc(ulong netID)
    {
        Debug.Log("Probáljunk meg letenni egy Orb-ot");
        GameObject go = Instantiate(ExtraPointGameObject);
        go.GetComponent<NetworkObject>().SpawnWithOwnership(netID);
        ulong itemNetID = go.GetComponent<NetworkObject>().NetworkObjectId;

        SpawnExtrapointClientRpc(itemNetID);
    }

    [ClientRpc]
    private void SpawnExtrapointClientRpc(ulong itemNetID)
    {
        Debug.Log("Sikerült letenni egy Orb-ot");
        NetworkObject netObj = NetworkManager.SpawnManager.SpawnedObjects[itemNetID];
    }


    public void GetExtraPoint(ulong itemNetID)
    {
        Debug.Log("GetExtraPoint");
        if (NetworkManager.IsClient)
            DeSpawnExtrapointServerRpc(itemNetID);
    }


    [ServerRpc]
    private void DeSpawnExtrapointServerRpc(ulong netID)
    {
        Debug.Log("GetExtraPoint");
        NetworkManager.SpawnManager.SpawnedObjects[netID].RemoveOwnership();
        NetworkManager.SpawnManager.SpawnedObjects[netID].Despawn();
        PointsNet.Value += 5f;
        points += 5f;
        //Debug.Log("Probáljunk meg felszedni az Orb-ot");
        //GameObject go = Instantiate(ExtraPointGameObject);
        //go.GetComponent<NetworkObject>().SpawnWithOwnership(netID);
        //ulong itemNetID = go.GetComponent<NetworkObject>().NetworkObjectId;

        //SpawnExtrapointClientRpc(itemNetID);
    }



    private void EarnPoints()
    {
        //Debug.Log("EarnPoints");
        if (transform.position.magnitude <= 2)
        {
            
            InnerCircleServerRpc();
        }
    }

    [ServerRpc]
    private void InnerCircleServerRpc()
    {
        //Debug.Log("Inner");
        float plusPoints = Time.deltaTime;
        PointsNet.Value += plusPoints;
        points += plusPoints;
    }

    public void FixedUpdate()
    {
        //myMaterial = colors[GameObject.FindGameObjectsWithTag("BoboPlayer").Length];
        if (IsOwner)
        {
            if (Input.GetKey(KeyCode.K))
                myMaterial.color = Color.black;
            EarnPoints();
            //networkTransform.Interpolate = !networkTransform.Interpolate;
            //networkTransform.
            //Debug.Log("OK");
            /*
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
                WASDSS[1] = Input.GetKey(KeyCode.A);ww
                WASDSS[3] = Input.GetKey(KeyCode.D);
                WASDSS[4] = Input.GetKey(KeyCode.LeftShift);
                WASDSS[5] = Input.GetKey(KeyCode.Space);
            }
            */
            if (IsOwner)
            {
                GetInputServerRpc(Input.GetKey(KeyCode.W), Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.S), Input.GetKey(KeyCode.D));
                
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
        }

    }

    [ClientRpc]
    public void UpdateMapClientRpc(Vector3[] cpoints, Vector3[] rpoints, Vector3[] ugratok_pos, Quaternion[] ugratok_qua, ClientRpcParams rpcParams = default) // Vector3[] ugratok_pos, Quaternion[] ugratok_qua,
    {
        Debug.Log("hm");
        //PathCreation.PathCreator creator = GameObject.FindGameObjectWithTag("RoadCreator").GetComponent<RoadMeshCreator>().pathCreator;
        if (!IsServer && IsClient)
        {
            Debug.Log("MapUpdate");
            RoadMeshCreator creator = GameObject.FindGameObjectWithTag("RoadCreator").GetComponent<RoadMeshCreator>();
            creator.Hapci(cpoints, rpoints);

            for (int i = 0; i < ugratok_pos.Length; i++)
            {
                Instantiate(creator.ramp, ugratok_pos[i], ugratok_qua[i]);
            }

            Debug.Log("Sides");
            RoadSideMeshCreatorA creatorA = GameObject.FindGameObjectWithTag("SideA").GetComponent<RoadSideMeshCreatorA>();
            RoadSideMeshCreatorB creatorB = GameObject.FindGameObjectWithTag("SideB").GetComponent<RoadSideMeshCreatorB>();
            creatorA.Hapci();
            creatorB.Hapci();


            Debug.Log("Invis");
            
            InvisibleSideMeshCreatorA invcreatorA = GameObject.FindGameObjectWithTag("InvSideA").GetComponent<InvisibleSideMeshCreatorA>();
            InvisibleSideMeshCreatorB invcreatorB = GameObject.FindGameObjectWithTag("InvSideB").GetComponent<InvisibleSideMeshCreatorB>();
            invcreatorA.Hapci();
            invcreatorB.Hapci();

            Debug.Log("PowerUps");
            GeneratePowerups script = GameObject.FindGameObjectWithTag("PowGen").GetComponent<GeneratePowerups>();
            script.Hapci();
        }
    }

    public void Update()
    {
        
        if (IsOwner)
        {
            //networkTransform.Interpolate = !networkTransform.Interpolate;
            //networkTransform.
            //Debug.Log("OK");

            if (NetworkManager.Singleton.IsServer)
            {
                Vector3 velo = MoveRly();
                rigi.AddForceAtPosition(velo, transform.position);
                if (Input.GetKeyDown(KeyCode.M))
                {
                    Debug.Log("M pressed");
                    SpawnExtrapointServerRpc(NetworkManager.Singleton.LocalClientId);
                }
            }
            else
            {
                MoveRlyRequestServerRpc();
                rigi.AddForceAtPosition(Velocity.Value,transform.position);

                
            }
        }
        
    }
}
